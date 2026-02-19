"""
Serveur de licences pour Logiciel d'impression 3D
FastAPI + SQLite — déployé sur Synology DS1019+ via Docker
"""

import os
import random
import secrets
from datetime import datetime, timedelta

import urllib.parse

from fastapi import FastAPI, HTTPException, Header, Request, Response
from fastapi.responses import HTMLResponse, RedirectResponse
from fastapi.templating import Jinja2Templates
from pydantic import BaseModel

from database import (
    initialiser_db,
    obtenir_licence_par_cle,
    obtenir_licence_par_token,
    activer_licence,
    mettre_a_jour_verification,
    marquer_scratch_revele,
    inserer_licence,
    revoquer_licence,
    lister_licences,
    inserer_utilisateur,
    obtenir_utilisateur_par_email,
    obtenir_utilisateur_par_nom,
    obtenir_utilisateur_par_token,
    verifier_utilisateur,
    lister_utilisateurs,
    definir_role,
    paiement_existe,
    enregistrer_paiement,
)
from email_service import email_verification_compte, email_contact_admin, email_licence
from paypal_service import valider_ipn

# ── Configuration ────────────────────────────────────────────────────────────
ADMIN_KEY          = os.environ.get("ADMIN_KEY",           "changez-moi-en-production")
PAYPAL_EMAIL       = os.environ.get("PAYPAL_EMAIL",        "").strip().lower()
PAYPAL_MODE        = os.environ.get("PAYPAL_MODE",         "sandbox")
PAYPAL_PRIX_MENSUEL = os.environ.get("PAYPAL_PRIX_MENSUEL", "9.99")
APP_VERSION        = "1.0.0"

app = FastAPI(title="Serveur de licences", docs_url=None, redoc_url=None)
templates = Jinja2Templates(directory="/app/templates")

# ── Démarrage ────────────────────────────────────────────────────────────────
@app.on_event("startup")
def startup():
    initialiser_db()


# ── Modèles Pydantic ─────────────────────────────────────────────────────────
class RequeteActivation(BaseModel):
    cle: str
    machine_id: str


class RequeteVerification(BaseModel):
    cle: str
    machine_id: str


class RequeteGeneration(BaseModel):
    email: str
    type_licence: str = "monthly"   # "monthly" | "lifetime"
    duree_jours: int = 30


class RequeteRevocation(BaseModel):
    cle: str


class RequeteInscription(BaseModel):
    nom_utilisateur: str
    email: str


class RequeteVerificationCode(BaseModel):
    email: str
    code: str


class RequeteContact(BaseModel):
    email_expediteur: str
    sujet: str
    message: str


# ── PayPal ───────────────────────────────────────────────────────────────────
@app.get("/acheter", response_class=HTMLResponse)
def page_achat(request: Request):
    """Page publique d'achat de licence via PayPal."""
    paypal_url = (
        "https://www.sandbox.paypal.com/cgi-bin/webscr"
        if PAYPAL_MODE == "sandbox"
        else "https://www.paypal.com/cgi-bin/webscr"
    )
    return templates.TemplateResponse("acheter.html", {
        "request":      request,
        "paypal_url":   paypal_url,
        "paypal_email": PAYPAL_EMAIL,
        "paypal_mode":  PAYPAL_MODE,
        "prix_mensuel": PAYPAL_PRIX_MENSUEL,
        "notify_url":   f"{SERVEUR_URL}/api/paypal/ipn",
        "return_url":   f"{SERVEUR_URL}/paiement-succes",
        "cancel_url":   f"{SERVEUR_URL}/paiement-annule",
    })


@app.get("/paiement-succes", response_class=HTMLResponse)
def paiement_succes(request: Request):
    """Page affichée après un paiement réussi."""
    return templates.TemplateResponse("paiement_succes.html", {"request": request, "succes": True})


@app.get("/paiement-annule", response_class=HTMLResponse)
def paiement_annule(request: Request):
    """Page affichée si le paiement est annulé."""
    return templates.TemplateResponse("paiement_succes.html", {"request": request, "succes": False})


@app.post("/api/paypal/test-paiement")
def paypal_test_paiement(email: str, x_admin_key: str = Header(None)):
    """
    Simule un paiement PayPal (sandbox uniquement, accès admin).
    Génère une clé et envoie l'email — sans passer par PayPal.
    """
    verifier_admin(x_admin_key)
    if PAYPAL_MODE != "sandbox":
        raise HTTPException(status_code=403, detail="Endpoint de test désactivé en production")

    import uuid
    txn_id_test = "TEST-" + uuid.uuid4().hex[:16].upper()
    type_licence = "monthly"
    duree_jours  = 30
    mc_gross     = PAYPAL_PRIX_MENSUEL
    mc_currency  = "EUR"

    if paiement_existe(txn_id_test):
        raise HTTPException(status_code=409, detail="Transaction test déjà existante")

    alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"
    cle = "-".join(
        "".join(secrets.choice(alphabet) for _ in range(4))
        for _ in range(4)
    )
    scratch_token   = secrets.token_urlsafe(32)
    date_expiration = (datetime.utcnow() + timedelta(days=duree_jours)).isoformat()

    inserer_licence(cle, email, type_licence, date_expiration, scratch_token)
    enregistrer_paiement(txn_id_test, email, mc_gross, mc_currency, type_licence, cle)

    lien_scratch = f"/scratch/{scratch_token}"
    envoye = email_licence(email, cle, lien_scratch, type_licence, date_expiration)

    return {
        "txn_id":          txn_id_test,
        "cle":             cle,
        "email":           email,
        "date_expiration": date_expiration,
        "scratch_url":     f"{SERVEUR_URL}{lien_scratch}",
        "email_envoye":    envoye
    }


@app.post("/api/paypal/ipn")
async def paypal_ipn(request: Request):
    """Réception et traitement des notifications IPN PayPal."""
    raw_body = await request.body()

    # Toujours répondre 200 en premier (exigence PayPal)
    # Validation asynchrone
    if not valider_ipn(raw_body):
        print("⚠️  IPN PayPal rejeté (non vérifié)")
        return Response(status_code=200)

    # Parser les paramètres IPN
    params = urllib.parse.parse_qs(raw_body.decode("utf-8"), keep_blank_values=True)
    def get(key):
        return params.get(key, [None])[0] or ""

    payment_status  = get("payment_status")
    receiver_email  = get("receiver_email").strip().lower()
    txn_id          = get("txn_id")
    payer_email     = get("payer_email").strip()
    custom          = get("custom") or "monthly:30"
    mc_gross        = get("mc_gross") or "0"
    mc_currency     = get("mc_currency") or "EUR"

    # Vérifier que le paiement est bien pour nous
    if PAYPAL_EMAIL and receiver_email != PAYPAL_EMAIL:
        print(f"⚠️  IPN: receiver_email inattendu ({receiver_email})")
        return Response(status_code=200)

    if payment_status != "Completed":
        print(f"IPN reçu — statut ignoré : {payment_status}")
        return Response(status_code=200)

    if not txn_id or not payer_email:
        print("⚠️  IPN: txn_id ou payer_email manquant")
        return Response(status_code=200)

    # Anti-doublon
    if paiement_existe(txn_id):
        print(f"IPN: transaction déjà traitée : {txn_id}")
        return Response(status_code=200)

    # Parser le champ custom : "monthly:30"
    parts = custom.split(":")
    type_licence = parts[0] if parts[0] in ("monthly", "lifetime") else "monthly"
    try:
        duree_jours = int(parts[1]) if len(parts) >= 2 else 30
    except ValueError:
        duree_jours = 30

    # Générer la clé de licence
    alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"
    cle = "-".join(
        "".join(secrets.choice(alphabet) for _ in range(4))
        for _ in range(4)
    )
    scratch_token   = secrets.token_urlsafe(32)
    date_expiration = (datetime.utcnow() + timedelta(days=duree_jours)).isoformat()

    inserer_licence(cle, payer_email, type_licence, date_expiration, scratch_token)
    enregistrer_paiement(txn_id, payer_email, mc_gross, mc_currency, type_licence, cle)

    # Envoyer l'email avec la scratch card
    lien_scratch = f"/scratch/{scratch_token}"
    envoye = email_licence(payer_email, cle, lien_scratch, type_licence, date_expiration)

    print(f"✅ Paiement {txn_id} traité — clé {cle} {'envoyée' if envoye else 'générée (SMTP absent)'} à {payer_email}")
    return Response(status_code=200)


# ── Endpoints publics ────────────────────────────────────────────────────────
@app.post("/api/contact")
def contact_admin(req: RequeteContact):
    """Transmet un message utilisateur à l'administrateur par email."""
    email = req.email_expediteur.strip()
    sujet = req.sujet.strip()
    message = req.message.strip()

    if not email or not sujet or not message:
        raise HTTPException(status_code=400, detail="Tous les champs sont requis")

    if len(message) > 5000:
        raise HTTPException(status_code=400, detail="Message trop long (max 5000 caractères)")

    envoye = email_contact_admin(email, sujet, message)
    if not envoye:
        # SMTP absent ou ADMIN_EMAIL non configuré — on accepte quand même
        print(f"Contact non envoyé par email (SMTP/ADMIN_EMAIL) — de {email} : {sujet}")

    return {"message": "Message transmis à l'administration", "email_envoye": envoye}


@app.post("/api/activer")
def activer(req: RequeteActivation):
    """Première activation d'une clé sur un PC."""
    cle = req.cle.upper().strip()
    licence = obtenir_licence_par_cle(cle)

    if not licence:
        raise HTTPException(status_code=404, detail="Clé introuvable")

    if licence["statut"] == "revoked":
        raise HTTPException(status_code=403, detail="Clé révoquée")

    if licence["statut"] == "expired":
        raise HTTPException(status_code=403, detail="Clé expirée")

    # Vérifier expiration
    date_exp = datetime.fromisoformat(licence["date_expiration"])
    if datetime.utcnow() > date_exp:
        raise HTTPException(status_code=403, detail="Clé expirée")

    # Vérifier machine binding
    if licence["machine_id"] and licence["machine_id"] != req.machine_id:
        raise HTTPException(status_code=403, detail="Clé déjà activée sur un autre PC")

    # Première activation : lier la machine
    if not licence["machine_id"]:
        activer_licence(cle, req.machine_id)

    mettre_a_jour_verification(cle)

    return {
        "statut": "active",
        "type": licence["type"],
        "date_expiration": licence["date_expiration"],
        "message": "Activation réussie"
    }


@app.post("/api/verifier")
def verifier(req: RequeteVerification):
    """Vérification périodique d'une licence déjà activée."""
    cle = req.cle.upper().strip()
    licence = obtenir_licence_par_cle(cle)

    if not licence:
        raise HTTPException(status_code=404, detail="Clé introuvable")

    if licence["statut"] == "revoked":
        raise HTTPException(status_code=403, detail="Clé révoquée")

    if licence["machine_id"] and licence["machine_id"] != req.machine_id:
        raise HTTPException(status_code=403, detail="Machine non autorisée")

    date_exp = datetime.fromisoformat(licence["date_expiration"])
    if datetime.utcnow() > date_exp:
        raise HTTPException(status_code=403, detail="Abonnement expiré")

    mettre_a_jour_verification(cle)

    return {
        "statut": "active",
        "type": licence["type"],
        "date_expiration": licence["date_expiration"],
        "jours_restants": (date_exp - datetime.utcnow()).days
    }


# ── Scratch card ─────────────────────────────────────────────────────────────
@app.get("/scratch/{token}", response_class=HTMLResponse)
def scratch_card(request: Request, token: str):
    """Page web interactive scratch card pour révéler la clé."""
    licence = obtenir_licence_par_token(token)

    if not licence:
        return HTMLResponse("<h1>Lien invalide</h1>", status_code=404)

    deja_revele = bool(licence["scratch_revealed"])
    cle = licence["cle"] if deja_revele else licence["cle"]  # toujours affiché après scratch JS

    return templates.TemplateResponse("scratch_card.html", {
        "request": request,
        "cle": cle,
        "email": licence["email"],
        "type": licence["type"],
        "date_expiration": licence["date_expiration"][:10],
        "deja_revele": deja_revele,
        "token": token,
    })


@app.post("/scratch/{token}/reveler")
def reveler_scratch(token: str):
    """Appelé par le JS scratch card quand l'utilisateur gratte suffisamment."""
    licence = obtenir_licence_par_token(token)
    if not licence:
        raise HTTPException(status_code=404, detail="Token invalide")
    marquer_scratch_revele(token)
    return {"ok": True}


# ── Endpoints admin ──────────────────────────────────────────────────────────
def verifier_admin(x_admin_key: str = Header(None)):
    if x_admin_key != ADMIN_KEY:
        raise HTTPException(status_code=401, detail="Clé admin invalide")


@app.post("/api/admin/generer")
def generer_cle(req: RequeteGeneration, x_admin_key: str = Header(None)):
    verifier_admin(x_admin_key)

    # Générer clé format XXXX-XXXX-XXXX-XXXX
    alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"  # sans O/0/I/1 (ambigus)
    cle = "-".join(
        "".join(secrets.choice(alphabet) for _ in range(4))
        for _ in range(4)
    )

    scratch_token = secrets.token_urlsafe(32)
    date_expiration = (datetime.utcnow() + timedelta(days=req.duree_jours)).isoformat()

    inserer_licence(cle, req.email, req.type_licence, date_expiration, scratch_token)

    return {
        "cle": cle,
        "email": req.email,
        "scratch_token": scratch_token,
        "date_expiration": date_expiration,
        "lien_scratch": f"/scratch/{scratch_token}"
    }


@app.post("/api/admin/revoquer")
def revoquer(req: RequeteRevocation, x_admin_key: str = Header(None)):
    verifier_admin(x_admin_key)
    cle = req.cle.upper().strip()
    licence = obtenir_licence_par_cle(cle)
    if not licence:
        raise HTTPException(status_code=404, detail="Clé introuvable")
    revoquer_licence(cle)
    return {"message": f"Clé {cle} révoquée"}


@app.get("/api/admin/licences")
def liste_licences(x_admin_key: str = Header(None)):
    verifier_admin(x_admin_key)
    licences = lister_licences()
    return [dict(row) for row in licences]


@app.get("/api/sante")
def sante():
    return {"statut": "ok", "version": APP_VERSION}


# ── Endpoints utilisateurs ───────────────────────────────────────────────────
@app.post("/api/utilisateurs/inscrire")
def inscrire_utilisateur(req: RequeteInscription):
    """Enregistre un nouvel utilisateur et envoie l'email de vérification."""
    email = req.email.strip().lower()
    nom   = req.nom_utilisateur.strip()

    if not nom or not email:
        raise HTTPException(status_code=400, detail="Nom et email requis")

    if obtenir_utilisateur_par_nom(nom):
        raise HTTPException(status_code=409, detail="Nom d'utilisateur déjà utilisé")

    if obtenir_utilisateur_par_email(email):
        raise HTTPException(status_code=409, detail="Email déjà utilisé")

    # Générer code 6 chiffres et token email
    code = str(random.randint(100000, 999999))
    token = secrets.token_urlsafe(32)
    expiration = (datetime.utcnow() + timedelta(minutes=30)).isoformat()

    inserer_utilisateur(nom, email, code, expiration, token)

    # Envoyer email (non bloquant si SMTP absent)
    envoye = email_verification_compte(email, nom, code, token)

    return {
        "message": "Inscription enregistrée. Vérifiez votre email.",
        "email_envoye": envoye
    }


@app.post("/api/utilisateurs/verifier-code")
def verifier_code(req: RequeteVerificationCode):
    """Vérifie le code à 6 chiffres saisi dans l'application."""
    email = req.email.strip().lower()
    utilisateur = obtenir_utilisateur_par_email(email)

    if not utilisateur:
        raise HTTPException(status_code=404, detail="Utilisateur introuvable")

    if utilisateur["verifie"]:
        return {"message": "Email déjà vérifié", "verifie": True}

    if not utilisateur["code_verification"]:
        raise HTTPException(status_code=400, detail="Aucun code en attente")

    expiration = datetime.fromisoformat(utilisateur["code_expiration"])
    if datetime.utcnow() > expiration:
        raise HTTPException(status_code=400, detail="Code expiré — demandez un renvoi")

    if utilisateur["code_verification"] != req.code.strip():
        raise HTTPException(status_code=400, detail="Code incorrect")

    verifier_utilisateur(email)
    return {"message": "Email vérifié avec succès", "verifie": True}


@app.get("/verifier-email/{token}", response_class=HTMLResponse)
def verifier_email_lien(request: Request, token: str):
    """Confirmation via le lien cliqué dans l'email."""
    utilisateur = obtenir_utilisateur_par_token(token)

    if not utilisateur:
        return templates.TemplateResponse("verifier_email.html", {
            "request": request, "succes": False,
            "message": "Lien invalide ou déjà utilisé."
        })

    if utilisateur["verifie"]:
        return templates.TemplateResponse("verifier_email.html", {
            "request": request, "succes": True,
            "message": "Votre email est déjà vérifié !"
        })

    expiration = datetime.fromisoformat(utilisateur["code_expiration"])
    if datetime.utcnow() > expiration:
        return templates.TemplateResponse("verifier_email.html", {
            "request": request, "succes": False,
            "message": "Lien expiré. Reconnectez-vous au logiciel pour recevoir un nouveau code."
        })

    verifier_utilisateur(utilisateur["email"])
    return templates.TemplateResponse("verifier_email.html", {
        "request": request, "succes": True,
        "message": f"Bonjour {utilisateur['nom_utilisateur']} ! Votre email est maintenant vérifié."
    })


@app.post("/api/utilisateurs/renvoyer-code")
def renvoyer_code(req: RequeteInscription):
    """Renvoie un nouveau code de vérification."""
    email = req.email.strip().lower()
    utilisateur = obtenir_utilisateur_par_email(email)

    if not utilisateur:
        raise HTTPException(status_code=404, detail="Utilisateur introuvable")

    if utilisateur["verifie"]:
        return {"message": "Email déjà vérifié"}

    code = str(random.randint(100000, 999999))
    token = secrets.token_urlsafe(32)
    expiration = (datetime.utcnow() + timedelta(minutes=30)).isoformat()
    inserer_utilisateur(req.nom_utilisateur or utilisateur["nom_utilisateur"], email, code, expiration, token)

    envoye = email_verification_compte(email, utilisateur["nom_utilisateur"], code, token)
    return {"message": "Nouveau code envoyé", "email_envoye": envoye}


# ── Dashboard admin web ──────────────────────────────────────────────────────
@app.get("/admin", response_class=HTMLResponse)
def admin_dashboard(request: Request, key: str = ""):
    """Dashboard d'administration web."""
    if key != ADMIN_KEY:
        return HTMLResponse("""
            <form method='get' style='font-family:sans-serif;padding:40px;max-width:400px;margin:auto'>
                <h2>🔐 Administration</h2>
                <input name='key' type='password' placeholder='Clé admin'
                       style='width:100%;padding:10px;margin:10px 0;border:1px solid #ccc;border-radius:6px'>
                <button type='submit' style='width:100%;padding:10px;background:#3498db;color:#fff;border:none;border-radius:6px;cursor:pointer'>
                    Connexion
                </button>
            </form>""", status_code=200)

    licences = [dict(row) for row in lister_licences()]
    utilisateurs = [dict(row) for row in lister_utilisateurs()]

    return templates.TemplateResponse("admin.html", {
        "request": request,
        "licences": licences,
        "utilisateurs": utilisateurs,
        "admin_key": key,
        "stats": {
            "total_utilisateurs": len(utilisateurs),
            "utilisateurs_verifies": sum(1 for u in utilisateurs if u["verifie"]),
            "total_licences": len(licences),
            "licences_actives": sum(1 for l in licences if l["statut"] == "active"),
        }
    })


# ── Endpoints admin utilisateurs ─────────────────────────────────────────────
@app.get("/api/admin/utilisateurs")
def liste_utilisateurs(x_admin_key: str = Header(None)):
    verifier_admin(x_admin_key)
    return [dict(row) for row in lister_utilisateurs()]


@app.post("/api/admin/verifier-utilisateur")
def forcer_verification(req: RequeteInscription, x_admin_key: str = Header(None)):
    verifier_admin(x_admin_key)
    utilisateur = obtenir_utilisateur_par_email(req.email)
    if not utilisateur:
        raise HTTPException(status_code=404, detail="Utilisateur introuvable")
    verifier_utilisateur(req.email)
    return {"message": f"Utilisateur {req.email} vérifié manuellement"}


@app.post("/api/admin/definir-role")
def changer_role(nom_utilisateur: str, role: str, x_admin_key: str = Header(None)):
    verifier_admin(x_admin_key)
    if role not in ("user", "admin"):
        raise HTTPException(status_code=400, detail="Rôle invalide (user ou admin)")
    definir_role(nom_utilisateur, role)
    return {"message": f"Rôle de {nom_utilisateur} → {role}"}

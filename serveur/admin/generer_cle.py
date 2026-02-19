#!/usr/bin/env python3
"""
Script admin pour générer une clé de licence et envoyer l'email scratch card.

Usage :
    python generer_cle.py --email client@example.com [--type monthly] [--jours 30]
    python generer_cle.py --email client@example.com --type lifetime

Variables d'environnement requises :
    ADMIN_KEY      Clé admin du serveur (même valeur que dans docker-compose.yml)
    SERVEUR_URL    URL du serveur (ex: https://licence.mondomaine.fr)
    SMTP_HOST      Serveur SMTP (ex: smtp.gmail.com)
    SMTP_PORT      Port SMTP (ex: 587)
    SMTP_USER      Adresse email expéditeur
    SMTP_PASS      Mot de passe SMTP / mot de passe d'application
    EMAIL_FROM     Nom affiché (ex: "Logiciel Impression 3D")
"""

import argparse
import json
import os
import smtplib
import sys
import urllib.request
import urllib.error
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

# ── Configuration (via variables d'environnement) ────────────────────────────
ADMIN_KEY   = os.environ.get("ADMIN_KEY",    "CHANGEZ-MOI-EN-PRODUCTION")
SERVEUR_URL = os.environ.get("SERVEUR_URL",  "https://licence.mondomaine.fr")
SMTP_HOST   = os.environ.get("SMTP_HOST",    "smtp.gmail.com")
SMTP_PORT   = int(os.environ.get("SMTP_PORT", "587"))
SMTP_USER   = os.environ.get("SMTP_USER",    "")
SMTP_PASS   = os.environ.get("SMTP_PASS",    "")
EMAIL_FROM  = os.environ.get("EMAIL_FROM",   "Logiciel Impression 3D")


def generer_cle_serveur(email: str, type_licence: str, duree_jours: int) -> dict:
    """Appelle l'API serveur pour générer une clé."""
    payload = json.dumps({
        "email": email,
        "type_licence": type_licence,
        "duree_jours": duree_jours
    }).encode()

    req = urllib.request.Request(
        f"{SERVEUR_URL}/api/admin/generer",
        data=payload,
        headers={
            "Content-Type": "application/json",
            "X-Admin-Key": ADMIN_KEY
        },
        method="POST"
    )

    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            return json.loads(resp.read())
    except urllib.error.HTTPError as e:
        print(f"Erreur HTTP {e.code}: {e.read().decode()}", file=sys.stderr)
        sys.exit(1)
    except urllib.error.URLError as e:
        print(f"Erreur connexion: {e.reason}", file=sys.stderr)
        sys.exit(1)


def envoyer_email(email_dest: str, cle: str, lien_scratch: str, type_licence: str, date_exp: str):
    """Envoie l'email avec le lien scratch card."""
    if not SMTP_USER or not SMTP_PASS:
        print("⚠️  Variables SMTP non configurées — email non envoyé.")
        print(f"   Lien scratch card : {SERVEUR_URL}{lien_scratch}")
        return

    msg = MIMEMultipart("alternative")
    msg["Subject"] = "🎉 Votre licence Logiciel d'impression 3D"
    msg["From"]    = f"{EMAIL_FROM} <{SMTP_USER}>"
    msg["To"]      = email_dest

    type_affiche = "Mensuel" if type_licence == "monthly" else "À vie"
    url_scratch  = f"{SERVEUR_URL}{lien_scratch}"

    html = f"""<!DOCTYPE html>
<html lang="fr">
<head><meta charset="UTF-8"></head>
<body style="font-family:'Segoe UI',sans-serif;background:#f5f5f5;margin:0;padding:20px;">
  <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);">

    <!-- Header -->
    <div style="background:linear-gradient(135deg,#1a1a2e,#0f3460);padding:32px 24px;text-align:center;">
      <h1 style="color:#fff;margin:0;font-size:1.4rem;">🖨️ Logiciel d'impression 3D</h1>
      <p style="color:rgba(255,255,255,0.7);margin:8px 0 0;font-size:0.9rem;">Votre licence est prête !</p>
    </div>

    <!-- Corps -->
    <div style="padding:32px 24px;">
      <p style="color:#333;font-size:0.95rem;margin:0 0 24px;">
        Merci pour votre achat ! Votre licence <strong>{type_affiche}</strong>
        (valable jusqu'au <strong>{date_exp[:10]}</strong>) vous attend.
      </p>

      <!-- Carte scratch -->
      <div style="background:linear-gradient(135deg,#f0f0f0,#e0e0e0);border-radius:12px;padding:24px;text-align:center;margin-bottom:28px;border:2px dashed #ccc;">
        <p style="color:#666;font-size:0.82rem;margin:0 0 12px;text-transform:uppercase;letter-spacing:0.1em;">🎰 Grattez pour révéler votre clé</p>
        <a href="{url_scratch}"
           style="display:inline-block;background:linear-gradient(135deg,#4facfe,#00f2fe);color:#0f3460;font-weight:700;font-size:1rem;padding:14px 32px;border-radius:10px;text-decoration:none;">
          ✦ Accéder à ma carte scratch
        </a>
        <p style="color:#999;font-size:0.78rem;margin:12px 0 0;">Grattez la zone grise pour révéler votre clé de licence</p>
      </div>

      <!-- Instructions -->
      <div style="background:#f8f9fa;border-radius:10px;padding:16px 20px;margin-bottom:20px;">
        <p style="color:#444;font-size:0.85rem;margin:0 0 8px;font-weight:600;">Comment activer votre licence :</p>
        <ol style="color:#555;font-size:0.85rem;margin:0;padding-left:20px;line-height:1.8;">
          <li>Cliquez sur « Accéder à ma carte scratch »</li>
          <li>Grattez la zone grise pour révéler votre clé</li>
          <li>Ouvrez le logiciel d'impression 3D</li>
          <li>Entrez la clé dans l'écran d'activation</li>
        </ol>
      </div>

      <p style="color:#999;font-size:0.78rem;margin:0;">
        ⚠️ Cette clé est liée à votre PC lors de la première activation.<br>
        Conservez cet email — votre clé : <code style="background:#f0f0f0;padding:2px 6px;border-radius:4px;">{cle}</code>
      </p>
    </div>

    <!-- Footer -->
    <div style="background:#f8f9fa;padding:16px 24px;text-align:center;border-top:1px solid #eee;">
      <p style="color:#aaa;font-size:0.75rem;margin:0;">
        Logiciel d'impression 3D &mdash; Pour toute question, répondez à cet email.
      </p>
    </div>
  </div>
</body>
</html>"""

    msg.attach(MIMEText(html, "html", "utf-8"))

    try:
        with smtplib.SMTP(SMTP_HOST, SMTP_PORT) as serveur:
            serveur.ehlo()
            serveur.starttls()
            serveur.login(SMTP_USER, SMTP_PASS)
            serveur.sendmail(SMTP_USER, email_dest, msg.as_string())
        print(f"✓ Email envoyé à {email_dest}")
    except Exception as e:
        print(f"⚠️  Erreur envoi email: {e}")
        print(f"   Lien scratch card : {url_scratch}")


def main():
    parser = argparse.ArgumentParser(description="Génère une clé de licence et envoie l'email scratch card")
    parser.add_argument("--email",  required=True,  help="Email du client")
    parser.add_argument("--type",   default="monthly", choices=["monthly", "lifetime"], help="Type de licence")
    parser.add_argument("--jours",  type=int, default=30, help="Durée en jours (défaut: 30)")
    args = parser.parse_args()

    print(f"→ Génération clé pour {args.email} ({args.type}, {args.jours} jours)...")
    result = generer_cle_serveur(args.email, args.type, args.jours)

    print(f"\n✓ Clé générée  : {result['cle']}")
    print(f"  Expiration   : {result['date_expiration'][:10]}")
    print(f"  Lien scratch : {SERVEUR_URL}{result['lien_scratch']}")

    print(f"\n→ Envoi de l'email...")
    envoyer_email(
        email_dest   = args.email,
        cle          = result["cle"],
        lien_scratch = result["lien_scratch"],
        type_licence = args.type,
        date_exp     = result["date_expiration"]
    )


if __name__ == "__main__":
    main()

"""
Service d'envoi d'emails partagé entre les licences et la vérification des comptes.
Configuration via variables d'environnement.
"""

import os
import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

SMTP_HOST  = os.environ.get("SMTP_HOST",  "smtp.gmail.com")
SMTP_PORT  = int(os.environ.get("SMTP_PORT", "587"))
SMTP_USER  = os.environ.get("SMTP_USER",  "")
SMTP_PASS  = os.environ.get("SMTP_PASS",  "")
EMAIL_FROM  = os.environ.get("EMAIL_FROM",  "Logiciel Impression 3D")
SERVEUR_URL = os.environ.get("SERVEUR_URL", "https://licence.mondomaine.fr")
ADMIN_EMAIL = os.environ.get("ADMIN_EMAIL", "")


def _smtp_disponible() -> bool:
    return bool(SMTP_USER and SMTP_PASS)


def envoyer_email(dest: str, sujet: str, corps_html: str) -> bool:
    """Envoie un email HTML. Retourne True si envoyé, False sinon."""
    if not _smtp_disponible():
        print(f"⚠️  SMTP non configuré — email non envoyé à {dest}")
        return False

    msg = MIMEMultipart("alternative")
    msg["Subject"] = sujet
    msg["From"]    = f"{EMAIL_FROM} <{SMTP_USER}>"
    msg["To"]      = dest
    msg.attach(MIMEText(corps_html, "html", "utf-8"))

    try:
        with smtplib.SMTP(SMTP_HOST, SMTP_PORT, timeout=15) as srv:
            srv.ehlo()
            srv.starttls()
            srv.login(SMTP_USER, SMTP_PASS)
            srv.sendmail(SMTP_USER, dest, msg.as_string())
        return True
    except Exception as e:
        print(f"⚠️  Erreur SMTP : {e}")
        return False


def email_contact_admin(email_expediteur: str, sujet: str, message: str) -> bool:
    """Envoie un message utilisateur à l'admin. L'adresse admin est invisible côté client."""
    if not ADMIN_EMAIL:
        print("⚠️  ADMIN_EMAIL non configuré — message non transmis")
        return False
    corps = f"""<!DOCTYPE html>
<html lang="fr"><head><meta charset="UTF-8"></head>
<body style="font-family:'Segoe UI',sans-serif;background:#f5f5f5;margin:0;padding:20px;">
  <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);">
    <div style="background:linear-gradient(135deg,#1a1a2e,#0f3460);padding:28px 24px;text-align:center;">
      <h1 style="color:#fff;margin:0;font-size:1.2rem;">🖨️ Logiciel d'impression 3D</h1>
      <p style="color:rgba(255,255,255,0.7);margin:6px 0 0;font-size:0.85rem;">Message d'un utilisateur</p>
    </div>
    <div style="padding:28px 24px;">
      <table style="width:100%;border-collapse:collapse;margin-bottom:20px;font-size:0.9rem;">
        <tr>
          <td style="color:#666;padding:6px 0;width:110px;font-weight:600;">Expéditeur :</td>
          <td style="color:#333;">{email_expediteur}</td>
        </tr>
        <tr>
          <td style="color:#666;padding:6px 0;font-weight:600;">Sujet :</td>
          <td style="color:#333;">{sujet}</td>
        </tr>
      </table>
      <div style="background:#f8f9fa;border-left:4px solid #3498db;border-radius:4px;padding:16px;font-size:0.9rem;color:#333;white-space:pre-wrap;">{message}</div>
      <p style="color:#aaa;font-size:0.78rem;margin-top:20px;">
        Pour répondre, utilisez directement l'adresse : <a href="mailto:{email_expediteur}" style="color:#3498db;">{email_expediteur}</a>
      </p>
    </div>
  </div>
</body></html>"""
    msg = MIMEMultipart("alternative")
    msg["Subject"] = f"[Contact logiciel] {sujet}"
    msg["From"]    = f"{EMAIL_FROM} <{SMTP_USER}>"
    msg["To"]      = ADMIN_EMAIL
    msg["Reply-To"] = email_expediteur
    msg.attach(MIMEText(corps, "html", "utf-8"))
    try:
        with smtplib.SMTP(SMTP_HOST, SMTP_PORT, timeout=15) as srv:
            srv.ehlo()
            srv.starttls()
            srv.login(SMTP_USER, SMTP_PASS)
            srv.sendmail(SMTP_USER, ADMIN_EMAIL, msg.as_string())
        return True
    except Exception as e:
        print(f"⚠️  Erreur SMTP contact admin : {e}")
        return False


def email_verification_compte(dest: str, nom_utilisateur: str, code: str, token: str) -> bool:
    """Email de vérification de compte (code + lien)."""
    lien = f"{SERVEUR_URL}/verifier-email/{token}"
    corps = f"""<!DOCTYPE html>
<html lang="fr"><head><meta charset="UTF-8"></head>
<body style="font-family:'Segoe UI',sans-serif;background:#f5f5f5;margin:0;padding:20px;">
  <div style="max-width:520px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);">
    <div style="background:linear-gradient(135deg,#1a1a2e,#0f3460);padding:28px 24px;text-align:center;">
      <h1 style="color:#fff;margin:0;font-size:1.3rem;">🖨️ Logiciel d'impression 3D</h1>
      <p style="color:rgba(255,255,255,0.7);margin:6px 0 0;font-size:0.85rem;">Vérification de votre adresse email</p>
    </div>
    <div style="padding:28px 24px;">
      <p style="color:#333;font-size:0.95rem;">Bonjour <strong>{nom_utilisateur}</strong>,</p>
      <p style="color:#555;font-size:0.9rem;margin-bottom:24px;">
        Merci de vous être inscrit. Vérifiez votre adresse email en utilisant le code ci-dessous
        ou en cliquant sur le lien de confirmation.
      </p>
      <!-- Code -->
      <div style="background:#f0f4ff;border-radius:12px;padding:20px;text-align:center;margin-bottom:20px;">
        <p style="color:#666;font-size:0.75rem;text-transform:uppercase;letter-spacing:0.1em;margin:0 0 8px;">Code de vérification</p>
        <div style="font-family:'Courier New',monospace;font-size:2.2rem;font-weight:700;color:#1a1a2e;letter-spacing:0.2em;">{code}</div>
        <p style="color:#999;font-size:0.75rem;margin:8px 0 0;">Valable 30 minutes — à saisir dans le logiciel</p>
      </div>
      <!-- Lien -->
      <div style="text-align:center;margin-bottom:24px;">
        <p style="color:#666;font-size:0.85rem;margin:0 0 12px;">Ou confirmez via votre navigateur :</p>
        <a href="{lien}" style="display:inline-block;background:linear-gradient(135deg,#3498db,#2980b9);color:#fff;font-weight:700;font-size:0.95rem;padding:12px 28px;border-radius:10px;text-decoration:none;">
          ✓ Confirmer mon email
        </a>
      </div>
      <p style="color:#aaa;font-size:0.78rem;">Si vous n'êtes pas à l'origine de cette inscription, ignorez cet email.</p>
    </div>
    <div style="background:#f8f9fa;padding:14px 24px;text-align:center;border-top:1px solid #eee;">
      <p style="color:#bbb;font-size:0.72rem;margin:0;">Logiciel d'impression 3D</p>
    </div>
  </div>
</body></html>"""
    return envoyer_email(dest, "✓ Vérifiez votre adresse email", corps)


def email_licence(dest: str, cle: str, lien_scratch: str, type_licence: str, date_exp: str) -> bool:
    """Email de livraison de licence (scratch card)."""
    type_affiche = "Mensuel" if type_licence == "monthly" else "À vie"
    url_scratch  = f"{SERVEUR_URL}{lien_scratch}"
    corps = f"""<!DOCTYPE html>
<html lang="fr"><head><meta charset="UTF-8"></head>
<body style="font-family:'Segoe UI',sans-serif;background:#f5f5f5;margin:0;padding:20px;">
  <div style="max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);">
    <div style="background:linear-gradient(135deg,#1a1a2e,#0f3460);padding:32px 24px;text-align:center;">
      <h1 style="color:#fff;margin:0;font-size:1.4rem;">🖨️ Logiciel d'impression 3D</h1>
      <p style="color:rgba(255,255,255,0.7);margin:8px 0 0;font-size:0.9rem;">Votre licence est prête !</p>
    </div>
    <div style="padding:32px 24px;">
      <p style="color:#333;font-size:0.95rem;margin:0 0 24px;">
        Merci pour votre achat ! Votre licence <strong>{type_affiche}</strong>
        (valable jusqu'au <strong>{date_exp[:10]}</strong>) vous attend.
      </p>
      <div style="background:linear-gradient(135deg,#f0f0f0,#e0e0e0);border-radius:12px;padding:24px;text-align:center;margin-bottom:28px;border:2px dashed #ccc;">
        <p style="color:#666;font-size:0.82rem;margin:0 0 12px;text-transform:uppercase;letter-spacing:0.1em;">🎰 Grattez pour révéler votre clé</p>
        <a href="{url_scratch}" style="display:inline-block;background:linear-gradient(135deg,#4facfe,#00f2fe);color:#0f3460;font-weight:700;font-size:1rem;padding:14px 32px;border-radius:10px;text-decoration:none;">
          ✦ Accéder à ma carte scratch
        </a>
      </div>
      <p style="color:#999;font-size:0.78rem;">Clé (pour référence) : <code style="background:#f0f0f0;padding:2px 6px;border-radius:4px;">{cle}</code></p>
    </div>
  </div>
</body></html>"""
    return envoyer_email(dest, "🎉 Votre licence Logiciel d'impression 3D", corps)

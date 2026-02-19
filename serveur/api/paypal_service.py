"""
Validation des notifications IPN PayPal.
"""

import os
import urllib.request
import urllib.parse

PAYPAL_MODE = os.environ.get("PAYPAL_MODE", "sandbox")

# URLs IPN PayPal
_URL_IPN = {
    "sandbox": "https://ipnpb.sandbox.paypal.com/cgi-bin/webscr",
    "live":    "https://ipnpb.paypal.com/cgi-bin/webscr",
}


def valider_ipn(raw_post: bytes) -> bool:
    """
    Valide une notification IPN PayPal en la renvoyant à PayPal.
    Retourne True si PayPal répond VERIFIED.
    """
    url = _URL_IPN.get(PAYPAL_MODE, _URL_IPN["sandbox"])
    data = b"cmd=_notify-validate&" + raw_post

    req = urllib.request.Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/x-www-form-urlencoded")
    req.add_header("User-Agent", "LogicielImpression3D-IPN/1.0")

    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            reponse = resp.read().decode("utf-8")
            if reponse != "VERIFIED":
                print(f"⚠️  IPN PayPal non vérifié : réponse = {reponse!r}")
                return False
            return True
    except Exception as e:
        print(f"⚠️  Erreur validation IPN PayPal : {e}")
        return False

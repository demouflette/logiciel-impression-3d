"""
Gestion de la base de données SQLite pour les licences.
"""

import sqlite3
import os
from datetime import datetime

DB_PATH = os.environ.get("DB_PATH", "/data/licences.db")


def obtenir_connexion():
    conn = sqlite3.connect(DB_PATH)
    conn.row_factory = sqlite3.Row
    return conn


def initialiser_db():
    """Crée les tables si elles n'existent pas."""
    with obtenir_connexion() as conn:
        conn.execute("""
            CREATE TABLE IF NOT EXISTS licences (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                cle TEXT NOT NULL UNIQUE,
                email TEXT NOT NULL,
                machine_id TEXT DEFAULT NULL,
                type TEXT NOT NULL DEFAULT 'monthly',
                date_creation TEXT NOT NULL,
                date_expiration TEXT NOT NULL,
                statut TEXT NOT NULL DEFAULT 'active',
                scratch_token TEXT NOT NULL UNIQUE,
                scratch_revealed INTEGER NOT NULL DEFAULT 0,
                derniere_verification TEXT DEFAULT NULL
            )
        """)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS utilisateurs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nom_utilisateur TEXT NOT NULL UNIQUE,
                email TEXT NOT NULL UNIQUE,
                role TEXT NOT NULL DEFAULT 'user',
                date_inscription TEXT NOT NULL,
                verifie INTEGER NOT NULL DEFAULT 0,
                code_verification TEXT DEFAULT NULL,
                code_expiration TEXT DEFAULT NULL,
                token_email TEXT DEFAULT NULL
            )
        """)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS paiements (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                txn_id TEXT NOT NULL UNIQUE,
                payer_email TEXT NOT NULL,
                montant TEXT NOT NULL,
                devise TEXT NOT NULL DEFAULT 'EUR',
                type_licence TEXT NOT NULL DEFAULT 'monthly',
                date_paiement TEXT NOT NULL,
                cle_generee TEXT DEFAULT NULL
            )
        """)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS sessions (
                session_id TEXT PRIMARY KEY,
                cle TEXT DEFAULT NULL,
                scratch_token TEXT DEFAULT NULL,
                statut TEXT NOT NULL DEFAULT 'pending',
                date_creation TEXT NOT NULL
            )
        """)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS promotions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                titre TEXT NOT NULL,
                message TEXT NOT NULL,
                type TEXT NOT NULL DEFAULT 'info',
                code TEXT DEFAULT NULL,
                jours_offerts INTEGER DEFAULT 0,
                date_creation TEXT NOT NULL,
                date_expiration TEXT NOT NULL,
                statut TEXT NOT NULL DEFAULT 'active'
            )
        """)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS promotions_cibles (
                promo_id INTEGER NOT NULL,
                email TEXT NOT NULL,
                PRIMARY KEY (promo_id, email)
            )
        """)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS promotions_utilisations (
                promo_id INTEGER NOT NULL,
                machine_id TEXT NOT NULL,
                email TEXT NOT NULL,
                date_utilisation TEXT NOT NULL,
                PRIMARY KEY (promo_id, machine_id)
            )
        """)
        conn.commit()


def obtenir_licence_par_cle(cle: str):
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT * FROM licences WHERE cle = ?", (cle,)
        ).fetchone()


def obtenir_licence_par_token(token: str):
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT * FROM licences WHERE scratch_token = ?", (token,)
        ).fetchone()


def activer_licence(cle: str, machine_id: str):
    """Lie la clé à un machine_id et met à jour la date d'activation."""
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE licences SET machine_id = ?, statut = 'active' WHERE cle = ?",
            (machine_id, cle)
        )
        conn.commit()


def mettre_a_jour_verification(cle: str):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE licences SET derniere_verification = ? WHERE cle = ?",
            (now, cle)
        )
        conn.commit()


def marquer_scratch_revele(token: str):
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE licences SET scratch_revealed = 1 WHERE scratch_token = ?",
            (token,)
        )
        conn.commit()


def inserer_licence(cle: str, email: str, type_licence: str, date_expiration: str, scratch_token: str):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT INTO licences (cle, email, type, date_creation, date_expiration, scratch_token)
               VALUES (?, ?, ?, ?, ?, ?)""",
            (cle, email, type_licence, now, date_expiration, scratch_token)
        )
        conn.commit()


def revoquer_licence(cle: str):
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE licences SET statut = 'revoked' WHERE cle = ?", (cle,)
        )
        conn.commit()


def lister_licences():
    with obtenir_connexion() as conn:
        return conn.execute("SELECT * FROM licences ORDER BY date_creation DESC").fetchall()


# ── Utilisateurs ─────────────────────────────────────────────────────────────

def inserer_utilisateur(nom: str, email: str, code: str, expiration: str, token: str):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT OR REPLACE INTO utilisateurs
               (nom_utilisateur, email, date_inscription, code_verification, code_expiration, token_email)
               VALUES (?, ?, ?, ?, ?, ?)""",
            (nom, email, now, code, expiration, token)
        )
        conn.commit()


def obtenir_utilisateur_par_email(email: str):
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT * FROM utilisateurs WHERE email = ?", (email,)
        ).fetchone()


def obtenir_utilisateur_par_nom(nom: str):
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT * FROM utilisateurs WHERE nom_utilisateur = ? COLLATE NOCASE", (nom,)
        ).fetchone()


def obtenir_utilisateur_par_token(token: str):
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT * FROM utilisateurs WHERE token_email = ?", (token,)
        ).fetchone()


def verifier_utilisateur(email: str):
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE utilisateurs SET verifie = 1, code_verification = NULL, token_email = NULL WHERE email = ?",
            (email,)
        )
        conn.commit()


def lister_utilisateurs():
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT id, nom_utilisateur, email, role, date_inscription, verifie FROM utilisateurs ORDER BY date_inscription DESC"
        ).fetchall()


def paiement_existe(txn_id: str) -> bool:
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT id FROM paiements WHERE txn_id = ?", (txn_id,)
        ).fetchone()
        return row is not None


def enregistrer_paiement(txn_id: str, payer_email: str, montant: str, devise: str,
                          type_licence: str, cle: str):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT OR IGNORE INTO paiements
               (txn_id, payer_email, montant, devise, type_licence, date_paiement, cle_generee)
               VALUES (?, ?, ?, ?, ?, ?, ?)""",
            (txn_id, payer_email, montant, devise, type_licence, now, cle)
        )
        conn.commit()


# ── Sessions PayPal ───────────────────────────────────────────────────────────

def creer_session(session_id: str):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            "INSERT OR IGNORE INTO sessions (session_id, statut, date_creation) VALUES (?, 'pending', ?)",
            (session_id, now)
        )
        conn.commit()


def lier_session_cle(session_id: str, cle: str, scratch_token: str):
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE sessions SET cle = ?, scratch_token = ?, statut = 'prete' WHERE session_id = ?",
            (cle, scratch_token, session_id)
        )
        conn.commit()


def obtenir_session(session_id: str):
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT cle, scratch_token FROM sessions WHERE session_id = ? AND statut = 'prete'",
            (session_id,)
        ).fetchone()
        return dict(row) if row else None


def definir_role(nom: str, role: str):
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE utilisateurs SET role = ? WHERE nom_utilisateur = ? COLLATE NOCASE", (role, nom)
        )
        conn.commit()


# ── Promotions ────────────────────────────────────────────────────────────────

def creer_promotion(titre: str, message: str, type_promo: str, code: str,
                    jours_offerts: int, date_expiration: str, emails: list):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        cur = conn.execute(
            """INSERT INTO promotions (titre, message, type, code, jours_offerts, date_creation, date_expiration, statut)
               VALUES (?, ?, ?, ?, ?, ?, ?, 'active')""",
            (titre, message, type_promo, code or None, jours_offerts, now, date_expiration)
        )
        promo_id = cur.lastrowid
        for email in emails:
            email = email.strip().lower()
            if email:
                conn.execute(
                    "INSERT OR IGNORE INTO promotions_cibles (promo_id, email) VALUES (?, ?)",
                    (promo_id, email)
                )
        conn.commit()
    return promo_id


def obtenir_promo_disponible(email: str, machine_id: str):
    """Retourne la première promo active disponible pour cet email/machine, ou None."""
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        row = conn.execute(
            """SELECT p.* FROM promotions p
               JOIN promotions_cibles pc ON pc.promo_id = p.id
               WHERE pc.email = ? COLLATE NOCASE
                 AND p.statut = 'active'
                 AND p.date_expiration > ?
                 AND NOT EXISTS (
                     SELECT 1 FROM promotions_utilisations pu
                     WHERE pu.promo_id = p.id AND pu.machine_id = ?
                 )
               ORDER BY p.date_creation DESC
               LIMIT 1""",
            (email, now, machine_id)
        ).fetchone()
        return dict(row) if row else None


def marquer_promo_utilisee(promo_id: int, machine_id: str, email: str):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT OR IGNORE INTO promotions_utilisations (promo_id, machine_id, email, date_utilisation)
               VALUES (?, ?, ?, ?)""",
            (promo_id, machine_id, email, now)
        )
        conn.commit()


def lister_promotions():
    with obtenir_connexion() as conn:
        rows = conn.execute(
            """SELECT p.*,
                      (SELECT COUNT(*) FROM promotions_utilisations pu WHERE pu.promo_id = p.id) AS nb_utilisations
               FROM promotions p
               ORDER BY p.date_creation DESC"""
        ).fetchall()
        return [dict(r) for r in rows]


def supprimer_promo(promo_id: int):
    """Désactive (supprime logiquement) une promotion."""
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE promotions SET statut = 'supprimee' WHERE id = ?", (promo_id,)
        )
        conn.commit()


def prolonger_licence(email: str, jours: int):
    """Ajoute `jours` jours à la date d'expiration de la licence active de cet email."""
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT id, date_expiration FROM licences WHERE email = ? COLLATE NOCASE AND statut = 'active' ORDER BY date_expiration DESC LIMIT 1",
            (email,)
        ).fetchone()
        if not row:
            return False
        date_exp = datetime.fromisoformat(row["date_expiration"])
        # Si déjà expirée, prolonger à partir d'aujourd'hui
        base = max(date_exp, datetime.utcnow())
        nouvelle_date = (base + timedelta(days=jours)).isoformat()
        conn.execute(
            "UPDATE licences SET date_expiration = ? WHERE id = ?",
            (nouvelle_date, row["id"])
        )
        conn.commit()
        return True

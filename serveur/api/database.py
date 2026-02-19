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


def definir_role(nom: str, role: str):
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE utilisateurs SET role = ? WHERE nom_utilisateur = ? COLLATE NOCASE", (role, nom)
        )
        conn.commit()

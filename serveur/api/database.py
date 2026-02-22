"""
Gestion de la base de données SQLite pour les licences.
"""

import hashlib
import sqlite3
import os
import secrets
import bcrypt
from datetime import datetime, timedelta

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
        # Table des sessions d'authentification (connexion depuis l'app)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS sessions_auth (
                session_id TEXT PRIMARY KEY,
                nom_utilisateur TEXT NOT NULL,
                date_creation TEXT NOT NULL,
                date_expiration TEXT NOT NULL
            )
        """)
        # Anti brute force : tentatives de connexion échouées
        conn.execute("""
            CREATE TABLE IF NOT EXISTS tentatives_connexion (
                nom_utilisateur TEXT PRIMARY KEY,
                nb_echecs INTEGER NOT NULL DEFAULT 0,
                derniere_tentative TEXT NOT NULL,
                bloque_jusqu TEXT DEFAULT NULL
            )
        """)
        # Journal des connexions (succès et échecs)
        conn.execute("""
            CREATE TABLE IF NOT EXISTS log_connexions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nom_utilisateur TEXT NOT NULL,
                ip TEXT,
                succes INTEGER NOT NULL,
                date TEXT NOT NULL,
                details TEXT
            )
        """)
        # Migrations : colonnes ajoutées après la création initiale
        colonnes_utilisateurs = [r["name"] for r in conn.execute("PRAGMA table_info(utilisateurs)")]
        if "password_hash" not in colonnes_utilisateurs:
            conn.execute("ALTER TABLE utilisateurs ADD COLUMN password_hash TEXT DEFAULT NULL")
        colonnes_sessions = [r["name"] for r in conn.execute("PRAGMA table_info(sessions_auth)")]
        if "user_agent" not in colonnes_sessions:
            conn.execute("ALTER TABLE sessions_auth ADD COLUMN user_agent TEXT DEFAULT NULL")
        colonnes_licences = [r["name"] for r in conn.execute("PRAGMA table_info(licences)")]
        if "alerte_expiration_envoyee" not in colonnes_licences:
            conn.execute("ALTER TABLE licences ADD COLUMN alerte_expiration_envoyee INTEGER NOT NULL DEFAULT 0")
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

def inserer_utilisateur(nom: str, email: str, code: str, expiration: str, token: str,
                        password_hash: str = None):
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT OR REPLACE INTO utilisateurs
               (nom_utilisateur, email, date_inscription, code_verification, code_expiration, token_email, password_hash)
               VALUES (?, ?, ?, ?, ?, ?, ?)""",
            (nom, email, now, code, expiration, token, password_hash)
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


def modifier_promo(promo_id: int, titre: str, message: str, type_promo: str,
                   code: str, jours_offerts: int, date_expiration: str, emails: list):
    with obtenir_connexion() as conn:
        conn.execute(
            """UPDATE promotions SET titre=?, message=?, type=?, code=?, jours_offerts=?, date_expiration=?
               WHERE id=?""",
            (titre, message, type_promo, code or None, jours_offerts, date_expiration, promo_id)
        )
        # Remplacer les emails ciblés
        conn.execute("DELETE FROM promotions_cibles WHERE promo_id=?", (promo_id,))
        for email in emails:
            email = email.strip().lower()
            if email:
                conn.execute(
                    "INSERT OR IGNORE INTO promotions_cibles (promo_id, email) VALUES (?, ?)",
                    (promo_id, email)
                )
        conn.commit()


def obtenir_emails_promo(promo_id: int):
    with obtenir_connexion() as conn:
        rows = conn.execute(
            "SELECT email FROM promotions_cibles WHERE promo_id=?", (promo_id,)
        ).fetchall()
        return [r["email"] for r in rows]


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


# ── Authentification utilisateurs ─────────────────────────────────────────────

def _hasher_password(password: str) -> str:
    """Hash un mot de passe avec bcrypt. Retourne le hash bcrypt."""
    return bcrypt.hashpw(password.encode("utf-8"), bcrypt.gensalt(rounds=12)).decode("utf-8")


def _verifier_hash(password: str, password_hash_stocke: str) -> bool:
    """Vérifie un mot de passe contre le hash stocké.
    Supporte bcrypt (nouveau format) et SHA-256 salé (ancien format 'sel:hash')."""
    if not password_hash_stocke:
        return False
    # Nouveau format bcrypt
    if password_hash_stocke.startswith("$2b$") or password_hash_stocke.startswith("$2a$"):
        return bcrypt.checkpw(password.encode("utf-8"), password_hash_stocke.encode("utf-8"))
    # Ancien format SHA-256 : 'sel:hash' — compatibilité migration
    if ":" in password_hash_stocke:
        sel, h = password_hash_stocke.split(":", 1)
        h_calcule = hashlib.sha256((sel + password).encode("utf-8")).hexdigest()
        return h_calcule == h
    return False


def verifier_credentials(nom_utilisateur: str, password: str):
    """Vérifie les identifiants. Retourne le dict utilisateur ou None.
    Si le hash est encore en ancien format SHA-256, le migre automatiquement vers bcrypt."""
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT * FROM utilisateurs WHERE nom_utilisateur = ? COLLATE NOCASE",
            (nom_utilisateur,)
        ).fetchone()
        if not row:
            return None
        h = row["password_hash"]
        if not _verifier_hash(password, h):
            return None
        # Migration transparente : si encore en SHA-256, re-hasher en bcrypt
        if h and not (h.startswith("$2b$") or h.startswith("$2a$")):
            conn.execute(
                "UPDATE utilisateurs SET password_hash = ? WHERE nom_utilisateur = ? COLLATE NOCASE",
                (_hasher_password(password), nom_utilisateur)
            )
            conn.commit()
        return dict(row)


def creer_compte_admin(nom: str, email: str, password: str) -> bool:
    """Crée un compte administrateur vérifié. Retourne False si le nom/email existe déjà."""
    with obtenir_connexion() as conn:
        existant = conn.execute(
            "SELECT id FROM utilisateurs WHERE nom_utilisateur = ? COLLATE NOCASE OR email = ? COLLATE NOCASE",
            (nom, email)
        ).fetchone()
        if existant:
            return False
        now = datetime.utcnow().isoformat()
        conn.execute(
            """INSERT INTO utilisateurs
               (nom_utilisateur, email, role, date_inscription, verifie, password_hash)
               VALUES (?, ?, 'admin', ?, 1, ?)""",
            (nom, email.strip().lower(), now, _hasher_password(password))
        )
        conn.commit()
    return True


def mettre_a_jour_password(nom_utilisateur: str, password: str):
    """Met à jour le hash du mot de passe d'un utilisateur existant."""
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE utilisateurs SET password_hash = ? WHERE nom_utilisateur = ? COLLATE NOCASE",
            (_hasher_password(password), nom_utilisateur)
        )
        conn.commit()


# ── Sessions d'authentification ───────────────────────────────────────────────

SESSION_DUREE_JOURS = 90  # durée de validité d'une session serveur


def creer_session_auth(session_id: str, nom_utilisateur: str, user_agent: str = None) -> str:
    """Crée une session d'authentification. Retourne la date d'expiration ISO."""
    now = datetime.utcnow()
    expiration = (now + timedelta(days=SESSION_DUREE_JOURS)).isoformat()
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT OR REPLACE INTO sessions_auth
               (session_id, nom_utilisateur, date_creation, date_expiration, user_agent)
               VALUES (?, ?, ?, ?, ?)""",
            (session_id, nom_utilisateur, now.isoformat(), expiration, user_agent)
        )
        conn.commit()
    return expiration


def obtenir_session_auth(session_id: str):
    """Retourne les infos utilisateur si la session est valide, sinon None."""
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        row = conn.execute(
            """SELECT u.nom_utilisateur, u.email, u.role, u.verifie,
                      s.date_expiration
               FROM sessions_auth s
               JOIN utilisateurs u ON u.nom_utilisateur = s.nom_utilisateur COLLATE NOCASE
               WHERE s.session_id = ? AND s.date_expiration > ?""",
            (session_id, now)
        ).fetchone()
    return dict(row) if row else None


def supprimer_session_auth(session_id: str):
    """Invalide une session (déconnexion)."""
    with obtenir_connexion() as conn:
        conn.execute("DELETE FROM sessions_auth WHERE session_id = ?", (session_id,))
        conn.commit()


def nettoyer_sessions_expirees():
    """Supprime les sessions expirées (à appeler périodiquement)."""
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        conn.execute("DELETE FROM sessions_auth WHERE date_expiration < ?", (now,))
        conn.commit()


def nettoyer_licences_obsoletes():
    """Supprime définitivement les licences expirées ou révoquées depuis plus de 15 jours."""
    with obtenir_connexion() as conn:
        conn.execute("""
            DELETE FROM licences
            WHERE date_expiration < datetime('now', '-15 days')
               OR (statut = 'revoked' AND date_creation < datetime('now', '-15 days'))
        """)
        conn.commit()


def nettoyer_promos_obsoletes():
    """Supprime définitivement les promotions expirées ou supprimées depuis plus de 15 jours,
    ainsi que leurs données liées (cibles et utilisations)."""
    with obtenir_connexion() as conn:
        rows = conn.execute("""
            SELECT id FROM promotions
            WHERE date_expiration < datetime('now', '-15 days')
               OR (statut = 'supprimee' AND date_creation < datetime('now', '-15 days'))
        """).fetchall()
        if rows:
            ids = [r[0] for r in rows]
            placeholders = ','.join('?' * len(ids))
            conn.execute(f"DELETE FROM promotions_utilisations WHERE promo_id IN ({placeholders})", ids)
            conn.execute(f"DELETE FROM promotions_cibles WHERE promo_id IN ({placeholders})", ids)
            conn.execute(f"DELETE FROM promotions WHERE id IN ({placeholders})", ids)
        conn.commit()


# ── Anti brute force ──────────────────────────────────────────────────────────

MAX_ECHECS     = 5     # tentatives avant blocage
BLOCAGE_MINUTES = 15  # durée du blocage


def verifier_blocage(nom_utilisateur: str) -> bool:
    """Retourne True si le compte est actuellement bloqué (trop de tentatives)."""
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT bloque_jusqu FROM tentatives_connexion WHERE nom_utilisateur = ? COLLATE NOCASE",
            (nom_utilisateur,)
        ).fetchone()
    if not row or not row["bloque_jusqu"]:
        return False
    return datetime.utcnow().isoformat() < row["bloque_jusqu"]


def enregistrer_echec_connexion(nom_utilisateur: str):
    """Incrémente le compteur d'échecs. Bloque le compte après MAX_ECHECS tentatives."""
    now = datetime.utcnow()
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT nb_echecs FROM tentatives_connexion WHERE nom_utilisateur = ? COLLATE NOCASE",
            (nom_utilisateur,)
        ).fetchone()
        nb = (row["nb_echecs"] + 1) if row else 1
        bloque_jusqu = None
        if nb >= MAX_ECHECS:
            bloque_jusqu = (now + timedelta(minutes=BLOCAGE_MINUTES)).isoformat()
        conn.execute(
            """INSERT INTO tentatives_connexion (nom_utilisateur, nb_echecs, derniere_tentative, bloque_jusqu)
               VALUES (?, ?, ?, ?)
               ON CONFLICT(nom_utilisateur) DO UPDATE SET
                   nb_echecs = excluded.nb_echecs,
                   derniere_tentative = excluded.derniere_tentative,
                   bloque_jusqu = excluded.bloque_jusqu""",
            (nom_utilisateur, nb, now.isoformat(), bloque_jusqu)
        )
        conn.commit()


def reinitialiser_tentatives(nom_utilisateur: str):
    """Remet à zéro le compteur après une connexion réussie."""
    with obtenir_connexion() as conn:
        conn.execute(
            "DELETE FROM tentatives_connexion WHERE nom_utilisateur = ? COLLATE NOCASE",
            (nom_utilisateur,)
        )
        conn.commit()


# ── Journal des connexions ─────────────────────────────────────────────────────

def ajouter_log_connexion(nom_utilisateur: str, ip: str, succes: bool, details: str = None):
    """Enregistre une tentative de connexion (réussie ou échouée)."""
    with obtenir_connexion() as conn:
        conn.execute(
            """INSERT INTO log_connexions (nom_utilisateur, ip, succes, date, details)
               VALUES (?, ?, ?, ?, ?)""",
            (nom_utilisateur, ip, 1 if succes else 0, datetime.utcnow().isoformat(), details)
        )
        conn.commit()


def nettoyer_logs_connexion():
    """Supprime les logs de connexion de plus de 30 jours."""
    with obtenir_connexion() as conn:
        conn.execute("DELETE FROM log_connexions WHERE date < datetime('now', '-30 days')")
        conn.commit()


def obtenir_logs_connexion(limit: int = 50):
    """Retourne les dernières tentatives de connexion."""
    with obtenir_connexion() as conn:
        return conn.execute(
            "SELECT * FROM log_connexions ORDER BY date DESC LIMIT ?", (limit,)
        ).fetchall()


def compter_connexions_aujourd_hui():
    """Retourne le nombre de connexions réussies aujourd'hui."""
    today = datetime.utcnow().isoformat()[:10]
    with obtenir_connexion() as conn:
        row = conn.execute(
            "SELECT COUNT(*) as nb FROM log_connexions WHERE date LIKE ? AND succes = 1",
            (today + "%",)
        ).fetchone()
        return row["nb"] if row else 0


# ── Alertes expiration de licence ─────────────────────────────────────────────

def obtenir_licences_expirant_bientot(jours: int = 7):
    """Retourne les licences actives qui expirent dans moins de `jours` jours
    et dont l'alerte email n'a pas encore été envoyée."""
    limite = (datetime.utcnow() + timedelta(days=jours)).isoformat()
    now = datetime.utcnow().isoformat()
    with obtenir_connexion() as conn:
        return conn.execute("""
            SELECT * FROM licences
            WHERE statut = 'active'
              AND date_expiration <= ?
              AND date_expiration >= ?
              AND alerte_expiration_envoyee = 0
        """, (limite, now)).fetchall()


def marquer_alerte_expiration(cle: str):
    """Marque la licence comme ayant reçu l'alerte d'expiration."""
    with obtenir_connexion() as conn:
        conn.execute(
            "UPDATE licences SET alerte_expiration_envoyee = 1 WHERE cle = ?", (cle,)
        )
        conn.commit()

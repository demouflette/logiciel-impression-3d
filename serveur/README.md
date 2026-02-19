# Serveur de licences — Logiciel d'impression 3D

Serveur FastAPI + SQLite pour la gestion des licences d'abonnement.
Déployé sur **Synology DS1019+** via Docker (Container Manager).

---

## Prérequis Synology

- Container Manager installé (DSM 7.2+)
- Accès SSH au Synology activé
- Reverse proxy Synology configuré (Control Panel → Login Portal → Advanced → Reverse Proxy)
- Certificat SSL Let's Encrypt pour `licence.mondomaine.fr`

---

## Déploiement initial

### 1. Copier les fichiers sur le Synology

```bash
# Depuis votre PC (PowerShell ou WSL)
scp -r serveur/ admin@synology.local:/volume1/docker/licences_impression_3d/
```

### 2. Configurer la clé admin

Éditez `docker-compose.yml` et changez la valeur de `ADMIN_KEY` :
```yaml
environment:
  - ADMIN_KEY=VotreCleSuperSecrete123!
  - DB_PATH=/data/licences.db
```

### 3. Lancer le conteneur

```bash
ssh admin@synology.local
cd /volume1/docker/licences_impression_3d
docker compose up -d
```

### 4. Vérifier que le serveur répond

```bash
curl http://localhost:8765/api/sante
# → {"statut": "ok", "version": "1.0.0"}
```

### 5. Configurer le reverse proxy Synology

Dans DSM → Control Panel → Login Portal → Advanced → Reverse Proxy :

| Champ | Valeur |
|---|---|
| Nom | licences_impression_3d |
| Source — Protocole | HTTPS |
| Source — Hostname | `licence.mondomaine.fr` |
| Source — Port | 443 |
| Destination — Protocole | HTTP |
| Destination — Hostname | localhost |
| Destination — Port | 8765 |

---

## Mise à jour du serveur

```bash
cd /volume1/docker/licences_impression_3d
docker compose pull
docker compose up -d --build
```

---

## Générer une clé et envoyer l'email

Depuis votre PC, avec Python 3 installé :

```bash
# Configurer les variables d'environnement
export ADMIN_KEY="VotreCleSuperSecrete123!"
export SERVEUR_URL="https://licence.mondomaine.fr"
export SMTP_HOST="smtp.gmail.com"
export SMTP_PORT="587"
export SMTP_USER="votre@gmail.com"
export SMTP_PASS="motdepasse-application-gmail"
export EMAIL_FROM="Logiciel Impression 3D"

# Générer une licence mensuelle (30 jours)
python admin/generer_cle.py --email client@example.com

# Générer une licence à vie
python admin/generer_cle.py --email client@example.com --type lifetime --jours 36500
```

> **Gmail** : utilisez un [mot de passe d'application](https://myaccount.google.com/apppasswords)
> (Compte Google → Sécurité → Validation en 2 étapes → Mots de passe des applications)

---

## Endpoints API

| Méthode | URL | Description |
|---|---|---|
| GET | `/api/sante` | Healthcheck |
| POST | `/api/activer` | Activation clé (1ère fois) |
| POST | `/api/verifier` | Vérification licence |
| GET | `/scratch/{token}` | Page scratch card |
| POST | `/scratch/{token}/reveler` | Marquer scratch comme révélé |
| POST | `/api/admin/generer` | Générer une clé (admin) |
| POST | `/api/admin/revoquer` | Révoquer une clé (admin) |
| GET | `/api/admin/licences` | Lister toutes les licences (admin) |

Les endpoints `/api/admin/*` nécessitent le header `X-Admin-Key: VotreCleSuperSecrete123!`

---

## Structure des données

```
data/
└── licences.db   ← Base SQLite (persistée via volume Docker)
```

**Table `licences` :**

| Colonne | Type | Description |
|---|---|---|
| id | INTEGER | ID auto |
| cle | TEXT | Clé XXXX-XXXX-XXXX-XXXX |
| email | TEXT | Email du client |
| machine_id | TEXT | Hash SHA256 du PC activé |
| type | TEXT | `monthly` ou `lifetime` |
| date_creation | TEXT | ISO 8601 UTC |
| date_expiration | TEXT | ISO 8601 UTC |
| statut | TEXT | `active`, `expired`, `revoked` |
| scratch_token | TEXT | Token URL unique de la scratch card |
| scratch_revealed | INTEGER | 0/1 — si la carte a été grattée |
| derniere_verification | TEXT | Dernière vérification client |

---

## Sécurité

- Toutes les communications client → serveur passent en **HTTPS** (Let's Encrypt)
- La clé admin n'est jamais exposée au client
- Le `machine_id` est un SHA256 irréversible (pas de données personnelles)
- La base de données est sur un volume Synology chiffré
- En cas de compromission de la `ADMIN_KEY`, changez-la et redémarrez le conteneur

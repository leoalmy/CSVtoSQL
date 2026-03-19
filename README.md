Un outil en ligne de commande rapide et robuste écrit en C# (.NET Framework 4.8) permettant de convertir des fichiers CSV en requêtes SQL d'insertion groupée (`INSERT INTO`).

## 🚀 Fonctionnalités

* **Performances optimisées** : Regroupe les insertions par lots de 1000 lignes (Batch Insert) pour une exécution ultra-rapide côté base de données.
* **Analyse intelligente (Regex)** : Gère parfaitement les séparateurs (`;`) inclus dans des chaînes de caractères entre guillemets.
* **Nettoyage des données** : Échappe automatiquement les apostrophes (`'`) et convertit les cellules vides en valeurs `NULL` (et non en chaînes vides).
* **Barre de progression** : Suivi visuel en temps réel du traitement pour les gros fichiers.
* **Automatisation** : Mode interactif avec confirmation de sécurité, ou mode silencieux (`-y`) pour l'intégration dans des scripts automatiques.

## ⚠️ Prérequis

* La table de destination **doit déjà exister** dans votre base de données.
* Le fichier CSV doit contenir **exactement le même nombre de colonnes**, dans le même ordre, que la table SQL cible.

## 💻 Utilisation

Ouvrez une invite de commande ou PowerShell et utilisez la syntaxe suivante :

```bash
CsvToSql.exe <fichier_entree.csv> <fichier_sortie.sql> [nom_table] [ignorer_1ere_ligne] [-y]
```

### Paramètres

| Paramètre | Requis | Description | Valeur par défaut |
| :--- | :---: | :--- | :--- |
| `fichier_entree.csv` | Oui | Le chemin vers votre fichier CSV source. | - |
| `fichier_sortie.sql` | Oui | Le chemin vers le fichier SQL à générer. | - |
| `nom_table` | Non | Le nom de la table SQL de destination. | *Nom du fichier CSV* |
| `ignorer_1ere_ligne` | Non | `true` pour ignorer les en-têtes, `false` pour tout lire. | `true` |
| `-y` | Non | Force l'exécution sans demander de confirmation à l'utilisateur. | - |

## 📖 Exemples

**1. Utilisation basique (avec les valeurs par défaut)**
Prend "data.csv", l'exporte en "export.sql", utilise "data" comme nom de table et ignore la première ligne.
```bash
CsvToSql.exe data.csv export.sql
```

**2. Spécifier un nom de table personnalisé**
```bash
CsvToSql.exe clients_2024.csv import.sql TableClients
```

**3. Traiter un fichier SANS en-têtes (comme employe.csv)**
Ici, on passe `false` pour indiquer au programme de lire dès la toute première ligne.
```bash
CsvToSql.exe employe.csv employe.sql Employes false
```

**4. Mode Automatique (Tâche planifiée ou script)**
Ajoute `-y` à la fin pour ignorer l'avertissement de sécurité.
```bash
CsvToSql.exe employe.csv employe.sql Employes false -y
```
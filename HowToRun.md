# Travail fait par :
Anthony Richer et Philippe Dion

Pour lancer le docker-compose il faut utiliser les commandes suivantes dans le folder ou se trouve le docker-compose.yml (eq22) : 
docker-compose build
docker-compose run client
Cela part les autres services du docker-compose tout en gardant le terminal pour les commandes du client.

### ** Note **
 Pour être capable de communiquer avec l'api de docker.donet (SUR LINUX), il faut écrire des trucs dans 2 fichiers pour être capable d'exposer daemon sur le localhost au port 2735. On a ainsi fait un script bash (worker-setup.sh) dans le répertoire root du projet (eq22/) qui doit être exécuté une fois avant de lancer l'application du worker pour s'assurer que ça fonctionne. J'utilise la commande avec sudo pour que ça fonctionne correctement (sudo bash worker-setup.sh)

Pour lancer l'application du worker sur un autre hôte, il faut se diriger au répertoire "eq22/Orchestrus" car c'est là où se trouve son fichier docker-compose.yml
Lancez les commandes suivantes pour bâtir l'image docker et la lancer :
sudo docker-compose build
sudo docker-compose up


### Les commandes
--help : commande d'aide pour afficher un menu avec les commandes disponibles
image : Afficher les images en exécution dans un worker spécifié avec son adresse IP
worker : Afficher les workers connectés
start : Partir une image par un travailleur en spécifiant la cartographie des ports
stop : Arrête le travail du travailleur spécifié par son adresse IP
t : Arreter l'application du client


### Tests et ci
Analyse statique du code: Une evaluation du code quality est fait lors du commit. Ce rapport est telechargable en allant sur le depot eq22 -> Pipeline -> codeQual (du dernier commit du TP3) -> Job artifacts -> dowload

** CI ** :
Nos stages de build et test de CI ne fonctionnent pas parce que nous avons pas été capable d'executer docker-compose dans le runner

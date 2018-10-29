########################################################
# Service config file: /etc/systemd/system/kestrel-docman.service
#
# Application folder: /var/apps/docman
#
##Testing Server install script
###inside_git_repo="$(git rev-parse --is-inside-work-tree 2>/dev/null)"
###if [ -d Galvarino/Galvarino.Web ]; then
###  # Control will enter here if $DIRECTORY exists.
###  cd Galvarino/Galvarino.Web
###  
###  if [ "$inside_git_repo" ]; then
###	echo "inside git repo"
###	#cd "$GIT_REPO_APP_DIR"
###	git fetch master
###	git checkout master
###  fi
###  
###  
###else
###	echo "not in git repo"
###	#git --version
###	
###	cd Galvarino/Galvarino.Web
###	git checkout master
###  
###fi

##git fetch master
##git checkout master



## $ sudo git config --global user.email "carlpradenas@gmail.com"
## $ sudo git config --global user.name "Carlos Pradenas"

## $ sudo git clone --single-branch -b master https://github.com/araucanosland/Galvarino.git
## $ sudo git stash save --keep-index --include-untracked
## $ sudo git checkout master
## $ sudo git pull
## $ sudo dotnet publish -o /home/desarrollo/Galvarino/output
## $ sudo rm -rfv /var/apps/docman/*
## $ sudo cp -rfv /home/desarrollo/Galvarino/output/* /var/apps/docman
## $ sudo systemctl restart kestrel-docman.service


## $ sudo chmod a+x /var/apps/docman/wwwroot/Rotativa
## $ sudo nano /etc/systemd/system/kestrel-docman.service
## $ sudo systemctl daemon-reload
## $ sudo systemctl restart kestrel-docman.service
## $ sudo systemctl status kestrel-docman.service


git checkout master

git stash save --keep-index --include-untracked

git pull

dotnet publish -o /home/desarrollo/Galvarino/output

rm -rfv /var/apps/docman/*

cp -rfv /home/desarrollo/Galvarino/output/* /var/apps/docman

systemctl restart kestrel-docman.service



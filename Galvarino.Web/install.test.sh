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
###	git clone --single-branch -b master https://github.com/araucanosland/Galvarino.git
###	cd Galvarino/Galvarino.Web
###	git checkout master
###  
###fi

##git fetch master
##git checkout master


git checkout master

git pull

dotnet publish -o /home/desarrollo/Galvarino/output

rm -rfv /var/apps/docman/*

cp -rfv /home/desarrollo/Galvarino/output/* /var/apps/docman

systemctl restart kestrel-docman.service
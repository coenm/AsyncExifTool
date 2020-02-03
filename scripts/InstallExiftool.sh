#!/bin/sh

# Get absolute path this script is in and use this path as a base for all other (relatve) filenames.
# !! Please make sure there are no spaces inside the path !!
# Source: https://stackoverflow.com/questions/242538/unix-shell-script-find-out-which-directory-the-script-file-resides
SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")
ROOT_PATH=$(cd ${SCRIPTPATH}/../; pwd)
EXIFTOOL_VERSION=$(cat "${ROOT_PATH}/EXIFTOOL_VERSION" | tr -d ' \n' | tr -d ' \r')
echo "Exiftool version to install: ${EXIFTOOL_VERSION}"

wget https://www.sno.phy.queensu.ca/~phil/exiftool/Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz
gzip -dc Image-ExifTool-${EXIFTOOL_VERSION}.tar.gz | tar -xf -
cd Image-ExifTool-${EXIFTOOL_VERSION}
perl Makefile.PL
# make test
sudo make install

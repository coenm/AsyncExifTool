$ErrorActionPreference = "Stop"

# Save the current location.
$CurrentDir = $(Get-Location).Path;
Write-Host "CurrentDir: " $CurrentDir

# Get location of powershell file
Write-Host "PSScriptRoot: " $PSScriptRoot

# we know this script is located in the .scripts\ folder of the root.
$RootDir = [IO.Path]::GetFullPath( (join-path $PSScriptRoot "..\") )
Write-Host "ROOT: " $RootDir


$exiftoolversion = [IO.File]::ReadAllText( (join-path $RootDir "EXIFTOOL_VERSION") )
$exiftoolversion = $exiftoolversion.Trim()
Write-Host "EXIFTOOL VERSION: " $exiftoolversion

$exiftoolZipFilename = "exiftool-" + $exiftoolversion + ".zip"
$destination = (join-path $CurrentDir $exiftoolZipFilename)

if ( !(Test-Path $destination) )
{
	$source = "http://www.sno.phy.queensu.ca/~phil/exiftool/" + $exiftoolZipFilename
	Write-Host "Downloading exiftool to " + $destination
	Write-Host " - Exiftool location: " $source
	
	# Tell powershell to use TLS 1.2 instad of default 1.0  and download the file.
	[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	Invoke-WebRequest $source -OutFile $destination
}
else 
{
	Write-Host "No need to download desired exiftool becase zip already exists."
}

$destinationDir = (join-path $CurrentDir "tools")
if ( !(Test-Path $destinationDir) )
{
	mkdir $destinationDir
}


# Assumes 7z in PATH
# TODO fix spaces in directories?!
$destDirArg="-o" + $destinationDir
#C:\Program` Files\7-Zip\7z.exe x $destination $destDirArg
7z.exe x $destination -aoa -y $destDirArg

$new = (join-path $destinationDir "exiftool.exe")
$old = (join-path $destinationDir "exiftool`(`-k`).exe")

if ( (Test-Path $old) )
{
	Write-Host "exiftool k exe exists"
	if ( (Test-Path $new) )
	{
		Write-Host "exiftool.exe exists -> remove"
		rm $new
	}
	Write-Host "rename to exiftool.exe"
	mv $old $new
}
else 
{
	Write-Host "----------------------"
	Write-Host "Could not find exiftool k .exe!!"
	Write-Host "----------------------"
}

dir $destinationDir

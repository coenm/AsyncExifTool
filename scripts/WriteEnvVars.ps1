#
# Script to be used in Azure DevOps.
# Reads all environment variables and produces a markdown file which is visible in the Build Summary.
# Might be useful for debugging purposes.
#
$var = (gci env:*).GetEnumerator() | Sort-Object Name
$out = ""
Foreach ($v in $var) {$out = $out + "`t{0,-28} = {1,-28}`n" -f $v.Name, $v.Value}

$fileName = (Join-Path $env:BUILD_ARTIFACTSTAGINGDIRECTORY "EnvironmentVariables.md")

write-output "Dump variables on $fileName"
set-content $fileName $out
 
write-output "##vso[task.addattachment type=Distributedtask.Core.Summary;name=Environment Variables;]$fileName"
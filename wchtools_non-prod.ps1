npx wchtools init --url https://content-eu-1.content-cms.com/api/37dd7bf6-5628-4aac-8464-f4894ddfb8c4 --user adarsh.bhautoo@hangarww.com --password Ad1108bh_hangarMU

$artifactPath = "$((Get-Location).path)\Artifacts\"
$assetList = $args[0]

Function PullAssets{
param($assetList)

	$assets = $assetList.split("|")
	foreach ($asset in $assets) {
		Write-Host "Processed $asset"
		npx wchtools pull -a --dir $artifactPath --path $asset -I --password Ad1108bh_hangarMU
	}
}

Function PushContents{
	npx wchtools push -c -f --dir $artifactPath --password Ad1108bh_hangarMU 
}

Function PushAssets{
	npx wchtools push -a -f --dir $artifactPath --password Ad1108bh_hangarMU -I
}

if(!$assetList -ne $null)
{
	PullAssets $assetList
	PushAssets
}

PushContents

Read-Host "Wait for a key press to exit"

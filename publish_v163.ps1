$token = "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"
$owner = "demouflette"
$repo  = "logiciel-impression-3d"
$exePath = "D:\projet_3d\logiciel d'impression 3d\bin\Release\logiciel d'impression 3d.exe"
$assetName = "logiciel-impression-3d.exe"
$headers = @{
    Authorization = "Bearer $token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

Write-Host "1. Creation release v1.6.3..."
$releaseBody = '{"tag_name":"v1.6.3","target_commitish":"master","name":"v1.6.3 - Systeme de promotions","body":"Ajout du systeme de promotions PROMO :\n- Bouton PROMO anime arc-en-ciel dans la barre de menu (cote Fichier)\n- Aide deplace a droite de la fenetre\n- Admin : creation et suppression de promotions depuis le dashboard\n- Promo type code : le code s affiche en grand dans l app + champ pre-rempli\n- Promo type info : message personnalise sans code\n- Verification au demarrage, une seule utilisation par machine","draft":true,"prerelease":false}'
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases" -Method POST -Headers $headers -Body $releaseBody -ContentType "application/json"
Write-Host "   ID: $($release.id)"

Write-Host "2. Upload exe..."
$uploadHeaders = @{
    Authorization = "Bearer $token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
    "Content-Type" = "application/octet-stream"
}
$exeBytes = [System.IO.File]::ReadAllBytes($exePath)
$asset = Invoke-RestMethod -Uri "https://uploads.github.com/repos/$owner/$repo/releases/$($release.id)/assets?name=$assetName" -Method POST -Headers $uploadHeaders -Body $exeBytes
Write-Host "   $($asset.name) - OK"

Write-Host "3. Publication..."
$pub = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases/$($release.id)" -Method PATCH -Headers $headers -Body '{"draft":false}' -ContentType "application/json"
Write-Host "SUCCES: $($pub.html_url)"

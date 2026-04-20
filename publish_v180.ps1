$token     = "TON_TOKEN_GITHUB_ICI"  # Remplacer par ton token GitHub (ne pas committer)
$owner     = "demouflette"
$repo      = "logiciel-impression-3d"
$setupPath = "D:\projet_3d\logiciel d'impression 3d\Installer\Crea_Cout_3D_Setup_1.8.0.0.exe"

$headers = @{
    Authorization = "Bearer $token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}
$uploadHeaders = @{
    Authorization = "Bearer $token"
    Accept = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
    "Content-Type" = "application/octet-stream"
}

Write-Host "1. Creation release v1.8.0..."
$releaseObj = @{
    tag_name         = "v1.8.0"
    target_commitish = "master"
    name             = "v1.8.0 - Corrections admin et panneau de gestion"
    body             = "### Corrections et nouvelles fonctionnalites admin`n`n- Admin : acces complet aux onglets 3MF et STL (plus de restriction d'abonnement)`n- Fix bug : code promo ne fonctionnait pas lors de l'activation (email manquant)`n- Admin : reinitialisation du mot de passe d'un utilisateur (clic droit dans le panneau)`n- Admin : ajout manuel de temps d'acces a un utilisateur (clic droit dans le panneau)"
    draft            = $true
    prerelease       = $false
}
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases" -Method POST -Headers $headers -Body ($releaseObj | ConvertTo-Json) -ContentType "application/json"
Write-Host "   ID: $($release.id)"

Write-Host "2. Upload setup.exe..."
$setupBytes = [System.IO.File]::ReadAllBytes($setupPath)
$a1 = Invoke-RestMethod -Uri "https://uploads.github.com/repos/$owner/$repo/releases/$($release.id)/assets?name=setup.exe" -Method POST -Headers $uploadHeaders -Body $setupBytes
Write-Host "   $($a1.name) - OK"

Write-Host "3. Upload installeur versionne..."
$a2 = Invoke-RestMethod -Uri "https://uploads.github.com/repos/$owner/$repo/releases/$($release.id)/assets?name=Crea_Cout_3D_Setup_1.8.0.0.exe" -Method POST -Headers $uploadHeaders -Body $setupBytes
Write-Host "   $($a2.name) - OK"

Write-Host "4. Publication..."
$pub = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases/$($release.id)" -Method PATCH -Headers $headers -Body '{"draft":false}' -ContentType "application/json"
Write-Host "SUCCES: $($pub.html_url)"

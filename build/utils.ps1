function Get-PropertyFromJson
{
    param (
        [string]$jsonFilePath,
        [string]$propertyName
    )

    # Read the JSON file content
    $jsonContent = Get-Content -Path $pluginJson -Raw

    # Parse the JSON content
    $jsonObject = ConvertFrom-Json -InputObject $jsonContent

    # Return the extracted property
    return $jsonObject.$propertyName
}

function Remove-Directory($path)
{
    if (Test-Path $path)
    {
        Remove-Item -Force -Recurse -Path "$path\*"
    }
}

function Print-Message
{
    param (
        [string]$message,
        [string]$color
    )

    Write-Host $message -ForegroundColor $color
}

function Print-Success
{
    param (
        [string]$message
    )

    Print-Message -message $message -color Green
}

function Print-Error
{
    param (
        [string]$message
    )

    Print-Message -message $message -color Red
}

function Print-Normal
{
    param (
        [string]$message
    )

    Print-Message -message $message -color White
}
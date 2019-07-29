param (
	[string]$IpAddress = "1.1.1.1"
)

$IP = $IpAddress
$MaskBits = 24 # This means subnet mask = 255.255.255.0
$Gateway = "20.30.40.1"
$Dns = "20.30.40.100"
$IPType = "IPv4"

# Retrieve the network adapter that you want to configure
$adapter = Get-NetAdapter | ? {$_.Status -eq "up"}

# Remove any existing IP, gateway from our ipv4 adapter
If (($adapter | Get-NetIPConfiguration).IPv4Address.IPAddress) {
    $adapter | Remove-NetIPAddress -AddressFamily $IPType -Confirm:$false
}
If (($adapter | Get-NetIPConfiguration).Ipv4DefaultGateway) {
    $adapter | Remove-NetRoute -AddressFamily $IPType -Confirm:$false
}

# Configure the IP address and default gateway
$adapter | New-NetIPAddress `
    -AddressFamily $IPType `
    -IPAddress $IP `
    -PrefixLength $MaskBits `
    -DefaultGateway $Gateway
# Configure the DNS client server IP addresses
$adapter | Set-DnsClientServerAddress -ServerAddresses $DNS

# Main structure (Header - fixed size: 112 bytes)
[magic: "SCDB" 4 bytes]
[version: u32]
[created: u64 (ms)]
[updated: u64 (ms)]
[tpm_sync_date: u64 (ms)]
[nonce_origin: 12 bytes]
[nonce_tpm: 12 bytes]
[reserved: 16 bytes]
[len_crypto_origin: u32]
[len_crypto_tpm: u32]
[origin_gcm_tag: 16 bytes]
[tpm_gcm_tag: 16 bytes]

# Data Area (Variable size)
[crypted_origin: byte[len_crypto_origin]]
[crypted_tpm: byte[len_crypto_tpm]]

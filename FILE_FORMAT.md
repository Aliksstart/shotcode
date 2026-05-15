# Main structure (Header - fixed size: 112 bytes)
|         Name         |  Value   |  Type  |    Size (bytes)   |
|----------------------|----------|--------| ----------------- |
| magic                |  "SCDB"  | byte[] |         4         |
| version              |    -     |  uint  |         4         |
| created              | u64 (ms) |  ulong |         8         |
| updated              | u64 (ms) |  ulong |         8         |
| tpm_sync_date        | u64 (ms) |  ulong |         8         |
| nonce_origin         |    bin   | byte[] |         12        |
| nonce_tpm            |    bin   | byte[] |         12        |
| reserved             |    bin   | byte[] |         16        |
| len_crypto_origin    |    bin   |  uint  |         4         |
| len_crypto_tpm       |    bin   |  uint  |         4         |
| origin_gcm_tag       |    bin   | byte[] |         16        |
| tpm_gcm_tag          |    bin   | byte[] |         16        |


## Data Area (Variable size)
|         Name         |  Value   |  Type  |    Size (bytes)   |
|----------------------|----------|--------| ----------------- |
| crypted_origin       |    bin   | byte[] | len_crypto_origin |
| crypted_tpm          |    bin   | byte[] |   len_crypto_tpm  |


# Element crypto origin
|         Name         |  Value   |  Type  |    Size (bytes)   |
|----------------------|----------|--------| ----------------- |
| argon2id_time_cost   |    -     |  uint  |         4         |
| argon2id_memory_cost |    -     |  uint  |         4         |
| argon2id_parallelism |    -     |  uint  |         4         |
| salt                 |    bin   | byte[] |         32        |
| count_blocks         |    -     |  uint  |         4         |
| blocks               |    bin   | byte[] |    count_blocks   |


# Description of each block.
|         Name         |  Value   |  Type  |    Size (bytes)   |
|----------------------|----------|--------| ----------------- |
| version              |    -     |  char  |         1         |
| type                 |    -     |  char  |         1         |
| digits               |    -     |  char  |         1         |
| algorithm            |    -     |  char  |         1         |
| ts_created           | u64 (ms) |  ulong |         8         |
| ts_updated           | u64 (ms) |  ulong |         8         |
| period_or_counter    |    -     |  ulong |         8         |
| len_service_name     |    -     |  uint  |         4         |
| len_secret           |    -     |  uint  |         4         |
| len_extra            |    -     |  uint  |         4         |
| service_name         |   UTF-8  | byte[] |    count_blocks   |
| secret               |    bin   | byte[] |     len_secret    |
| extra                |    bin   | byte[] |     len_extra     |

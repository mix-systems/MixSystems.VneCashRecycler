# Simulator Bundle Notes

The file `AC_SIM_2_10.rar` is a vendor-provided Windows simulator package.

## Archive contents

The extracted bundle contains:

- `AC_SIM.exe` as the simulator launcher
- `server.pem` for TLS-related assets
- `db.s3db` as the bundled SQLite database
- Qt / VC runtime files
- Python runtime files and packaged simulator scripts
- local log files produced by the simulator

## Local extraction path

The archive was extracted locally to:

- `docs/vendor/AC_SIM_2_10/`

This extracted directory is ignored by git on purpose.

## Operational note

After launching the simulator executable on Windows, the web service endpoint is
expected to be:

- `https://127.0.0.1/selfcashapi`

This matches the default endpoint used by the SDK examples and the embedded
integration-test harness.

# Vendor Integration Notes

Short summary of the vendor email received together with the protocol PDF and
the simulator archive.

## Minimum POS command set

The vendor explicitly called out these commands as the minimum set needed for a
POS integration:

- `tipo = 1` payment request
- `tipo = 2` payment status polling
- `tipo = 3` delete pending payment
- `tipo = 20` machine status

## Polling guidance

- The vendor recommended polling payment status every `250 ms`.
- The protocol document itself only says to wait a short interval before the
  next polling request.
- The SDK test harness follows the stricter vendor recommendation for payment
  polling.

## Machine status variability

- VNE warned that machines may expose `2`, `3`, `4`, or `6` banknote
  denominations.
- The `tipo = 20` status payload therefore contains a variable number of
  `banconota_*` tags.
- Coin and hopper information also varies by machine setup.

## WEB TOOL note

- VNE recommended exposing a button in the POS that opens the machine WEB TOOL
  via:
  - `https://<machine-ip>/tool`
- The SDK does not automate the WEB TOOL because it is an operational UI
  concern, not a protocol client concern.

## Simulator note

- The vendor simulator becomes reachable at:
  - `https://127.0.0.1/selfcashapi`
- after extracting the archive and launching the Windows executable.

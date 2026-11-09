# SDK Conformance Report

This note compares the current `MixSystems.VneCashRecycler` SDK against the vendor
materials currently stored in `docs/`.

## Source materials checked

- `VNE_RETAIL_communication_protocol_3_04-ENG.pdf`
- `AC_SIM_2_10.rar`
- vendor email summary captured in `vendor-integration-notes.md`

## Matches

### Transport and endpoint

- The SDK targets `https://<machine-ip>/selfcashapi/`.
- The default local simulator endpoint is `https://127.0.0.1/selfcashapi/`.
- Default timeout and retry behavior align with the protocol guidance of
  retrying after a `5s` no-response window.

### Minimum command set from the vendor email

The minimum integration calls are implemented:

- payment request (`tipo = 1`)
- payment status polling (`tipo = 2`)
- pending payment deletion (`tipo = 3`)
- machine status (`tipo = 20`)

### Additional protocol coverage

The SDK also covers the wider protocol surface from the PDF, including:

- pending payment list
- withdrawal flow
- refill flow
- emptying flow
- login / logout
- cash closing and reports
- configuration commands
- refund commands

### Variable machine-status payloads

- The SDK parses `banconota_*`, `moneta_*`, `hopper_*`, and stacker detail data
  through extension-data based mapping.
- This matches the vendor warning that machine status payloads vary by machine
  configuration and by number of banknote denominations.
- Integration tests now verify that variable banknote and coin tags are parsed
  correctly.

## Scope boundary

- The vendor recommended using the WEB TOOL for some operational tasks.
- The SDK intentionally focuses on the machine protocol and leaves WEB TOOL
  launching to the consuming application.

## Current verdict

No protocol-level blocker was found in the current SDK for the documented POS
integration flow. The implementation matches the vendor’s minimum command set
and already covers a broader part of the published protocol.

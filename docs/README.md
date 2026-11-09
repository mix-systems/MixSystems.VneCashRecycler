# Documentation

This folder stores the raw vendor materials and the short internal notes that
sit next to them.

## Vendor materials

- `VNE_RETAIL_communication_protocol_3_04-ENG.pdf`  
  Official VNE protocol document for the POS integration.
- `AC_SIM_2_10.rar`  
  Vendor-provided Windows simulator archive.

## Internal notes

- [vendor-integration-notes.md](vendor-integration-notes.md)  
  Short summary of the vendor email and the implementation-relevant guidance.
- [simulator-bundle-notes.md](simulator-bundle-notes.md)  
  Notes about the vendor simulator archive and its extracted contents.
- [sdk-conformance-report.md](sdk-conformance-report.md)  
  Current comparison between the SDK implementation and the vendor materials.

## Local extracted files

The simulator archive is intentionally extracted only into a local ignored
folder:

- `docs/vendor/AC_SIM_2_10/`

This keeps the repository clean while still allowing local inspection and
manual simulator runs.

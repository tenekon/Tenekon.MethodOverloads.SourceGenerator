# Unsupported scenarios

Short version: the generator only emits overloads for ordinary methods, and only by omitting a contiguous parameter window. Anything outside that model is not supported.

## Not supported

- Inheritance/override scenarios (base/derived methods).
- Constructors, operators, or other non-method members.
- Overloads that require parameter reordering or non-contiguous omission windows.
- Overloads that would drop a ref/out/in parameter.
- Target methods with default/optional parameters inside the window, or a params parameter outside the window (the entire method is skipped).
- Methods declared inside matcher types (matcher types are never targets).

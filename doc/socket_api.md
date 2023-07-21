# Socket API

This API operates over JSON-RPC.

## Protocol details

- All messages are to be formatted as MessagePack objects.
- Strings are represented in UTF-8.

The API shall be exposed as a Unix domain socket (henceforth UDS). The socket should be located under:

```
<m64+ cache path>/sockets/rpfw.sock
```

## Methods

```
ReadMemory({
    "addr": uint32_t
    "size": 1 | 2 | 4 | 8 | -1 | -2 | -4 | -8
}) returns {
    "value": <value>
}
```

`ReadMemory` takes an address and a size parameter. The size parameter represents the size of the desired read. If the
size is negated, the returned `value` is signed.

The value returned is of integer type and must be bit-cast to be used as a floating-point type.

```
WriteMemory({
    "addr": uint32_t
    "size": 1 | 2 | 4 | 8 | -1 | -2 | -4 | -8
    "value": <value>
}) returns void
```

`WriteMemory` takes an address and a size parameter. The size parameter represents the size of the desired read. If the
size is negated, `value` is interpreted as a signed integer.

The `value` must be supplied as an integer. If the value is out-of-range for the desired type, it will be truncated 
to the correct size.
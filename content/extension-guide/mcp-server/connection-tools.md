# Connection Tools

Connection tools manage the link between the AI assistant and the OneStream environment. They handle establishing connections, checking server health, and discovering available environments.

## connect_onestream

Establishes a connection to a OneStream environment. This must be called before using any other tools.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `url` | string | Yes | -- | The OneStream server URL (e.g., `https://yourcompany.onestream.com`) |
| `username` | string | Yes | -- | OneStream login username |
| `password` | string | Yes | -- | OneStream login password |
| `application` | string | No | -- | The application to connect to. If omitted, connects to the default application. |

### Return Value

Returns a connection status object:

```json
{
  "connected": true,
  "environment": "Production",
  "application": "GolfStream",
  "version": "8.2.0",
  "userId": "admin"
}
```

### Usage Example

```
Connect to the OneStream production environment at https://prod.onestream.com with user admin.
```

The AI assistant will call this tool to establish the connection before executing subsequent operations.

---

## health_check

Checks whether the current connection to OneStream is active and the server is responsive.

### Parameters

This tool takes no parameters.

### Return Value

Returns a health status object:

```json
{
  "healthy": true,
  "serverVersion": "8.2.0",
  "responseTimeMs": 142,
  "connectedSince": "2024-03-15T10:30:00Z"
}
```

If the connection is not healthy:

```json
{
  "healthy": false,
  "error": "Connection timed out",
  "lastHealthy": "2024-03-15T10:30:00Z"
}
```

### Usage Example

```
Check if the OneStream connection is still active.
```

---

## list_environments

Lists all OneStream environments (servers) that are configured or have been previously connected to.

### Parameters

This tool takes no parameters.

### Return Value

Returns an array of environment objects:

```json
[
  {
    "name": "Production",
    "url": "https://prod.onestream.com",
    "connected": true,
    "lastConnected": "2024-03-15T10:30:00Z"
  },
  {
    "name": "Development",
    "url": "https://dev.onestream.com",
    "connected": false,
    "lastConnected": "2024-03-14T08:00:00Z"
  }
]
```

### Usage Example

```
What OneStream environments are available?
```

---

## get_connection_info

Returns detailed information about the current active connection, including the environment, application, user, and session details.

### Parameters

This tool takes no parameters.

### Return Value

Returns a connection detail object:

```json
{
  "connected": true,
  "url": "https://prod.onestream.com",
  "environment": "Production",
  "application": "GolfStream",
  "username": "admin",
  "version": "8.2.0",
  "sessionId": "abc-123-def",
  "tokenExpiry": "2024-03-15T18:30:00Z"
}
```

### Usage Example

```
What OneStream environment am I currently connected to?
```

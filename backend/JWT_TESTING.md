# JWT Authentication Testing Guide

## Testing with Multiple Users

### Browser Window Requirements

⚠️ **Important**: To test with 2 different users simultaneously, you **MUST** use one of these approaches:

1. **Two Incognito/Private Windows** (Recommended)
   - Open 2 separate incognito/private browser windows
   - Each will have isolated cookies and local storage
   
2. **Different Browsers** 
   - Use Chrome in one browser and Firefox/Safari in another
   
3. **Different Browser Profiles**
   - Create separate browser profiles and open them simultaneously

❌ **Do NOT use regular tabs in the same browser window** - they share cookies and storage, making it impossible to maintain separate user sessions.

## API Endpoints

### Authentication Endpoints

#### Register User
```http
POST http://localhost:5016/api/auth/register
Content-Type: application/json

{
  "email": "user1@example.com",
  "userName": "user1",
  "password": "password123"
}
```

#### Login User
```http
POST http://localhost:5016/api/auth/login
Content-Type: application/json

{
  "email": "user1@example.com",
  "password": "password123"
}
```

Response includes JWT token:
```json
{
  "success": true,
  "user": {
    "id": 1,
    "email": "user1@example.com",
    "userName": "user1"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Chat Endpoints (Require Authorization)

All chat endpoints require the JWT token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

#### Get All Rooms
```http
GET http://localhost:5016/api/chat/rooms
Authorization: Bearer <your-jwt-token>
```

#### Get Messages from Room
```http
GET http://localhost:5016/api/chat/rooms/1/messages?count=50
Authorization: Bearer <your-jwt-token>
```

#### Send Message
```http
POST http://localhost:5016/api/chat/rooms/1/messages
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "content": "Hello, world!"
}
```

#### Create Room
```http
POST http://localhost:5016/api/chat/rooms
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "name": "General Discussion"
}
```

## Testing Workflow

1. **Register two different users** (or use existing ones)
2. **Open two incognito windows**
3. **Login each user in a separate window**
4. **Store the JWT tokens** from login responses
5. **Use the tokens** to access chat endpoints
6. **Test real-time communication** between the two users

## JWT Token Features

- **Expiry**: 60 minutes (configurable)
- **Claims**: User ID, Username, Email, JWT ID, Issued At
- **Security**: HMAC SHA256 signature
- **Validation**: Issuer, Audience, Lifetime, and Signature validation

## Swagger UI

Visit http://localhost:5016/swagger to test the API interactively with JWT authentication support.

1. Click "Authorize" button
2. Enter: `Bearer <your-jwt-token>`
3. Test endpoints directly in the browser

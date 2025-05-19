# BlockedCountriesWepApi

## Project Overview
This repository contains the **Blocked Countries Web API**, a .NET 8 Web API developed as an assignment for ATechnologies. The API provides a robust solution for managing a list of blocked countries and checking IP-based access restrictions based on geolocation. It allows administrators to block countries permanently or temporarily, check if an IP address belongs to a blocked country, and retrieve logs of access attempts. The project demonstrates adherence to SOLID principles, clean code practices, and efficient handling of external API rate limits.

### Objectives
- Enable blocking and unblocking of countries using 2-letter ISO country codes.
- Check if an IP address belongs to a blocked country using a geolocation service.
- Support temporary blocking with automatic cleanup of expired blocks.
- Log access attempts for monitoring and auditing purposes.
- Provide paginated and searchable lists of blocked countries and access logs.

## Technologies Used
- **.NET 8** with **ASP.NET Core** for building the Web API.
- **C#** for backend logic.
- **ConcurrentDictionary** and **ConcurrentBag** for thread-safe in-memory storage.
- **ipgeolocation.io** (with fallback to ipwhois.io) for geolocation data.
- **ILogger** for logging requests, errors, and system events.
- **Swagger** for API documentation and testing.
- **Dependency Injection** for loosely coupled services.
- **BackgroundService** for automatic cleanup of temporary blocks.

## API Endpoints
The API provides the following endpoints, all thoroughly tested using Swagger and cURL:

1. **GET /api/Ip/check-block?ipAddress={ip}**
   - Checks if the provided IP address belongs to a blocked country.
   - Response: `{ success: true, data: { IpAddress, IsBlocked, CountryCode, CountryName }, statusCode: 200 }`
   - Example: `curl -X GET "https://localhost:7029/api/Ip/check-block?ipAddress=1.1.1.1"`

2. **GET /api/Ip/lookup?ipAddress={ip}**
   - Retrieves geolocation details for an IP address.
   - Response: `{ success: true, data: { IpAddress, CountryCode, CountryName, Isp }, statusCode: 200 }`

3. **POST /api/Countries/block**
   - Blocks a country permanently.
   - Request: `{ "CountryCode": "EG" }`
   - Response: `{ success: true, data: null, statusCode: 200 }`

4. **POST /api/Countries/temporal-block**
   - Blocks a country temporarily for a specified duration (1-1440 minutes).
   - Request: `{ "CountryCode": "EG", "DurationMinutes": 60 }`
   - Response: `{ success: true, data: null, statusCode: 200 }`

5. **DELETE /api/Countries/block/{countryCode}**
   - Unblocks a country.
   - Response: `{ success: true, data: null, statusCode: 200 }`

6. **GET /api/Countries/blocked?page={page}&pageSize={pageSize}&search={search}**
   - Retrieves a paginated and searchable list of blocked countries.
   - Response: `{ success: true, data: { Page, PageSize, TotalCount, Items: [{ Code, Name }] }, statusCode: 200 }`

7. **GET /api/Countries/all-blocked**
   - Retrieves a simple list of all blocked countries without pagination.
   - Response: `{ success: true, data: [{ Code, Name }], statusCode: 200 }`

8. **GET /api/Countries/blocked-attempts?page={page}&pageSize={pageSize}**
   - Retrieves a paginated list of access attempt logs.
   - Response: `{ success: true, data: { Page, PageSize, TotalCount, Items: [{ IpAddress, Timestamp, CountryCode, IsBlocked, UserAgent }] }, statusCode: 200 }`

## Key Features
- **In-Memory Storage**: Uses `ConcurrentDictionary` for blocked countries and `ConcurrentBag` for attempt logs to ensure thread safety.
- **Geolocation Caching**: Implements caching in `GeoLocationService` to minimize external API calls and avoid rate limit errors (e.g., HTTP 423).
- **Automatic Cleanup**: A `BackgroundService` (`TemporalBlockCleanupService`) removes expired temporary blocks every 5 minutes.
- **Error Handling**: Comprehensive error handling with meaningful messages (e.g., 400 for invalid inputs, 409 for conflicts).
- **Logging**: Detailed logging of requests, errors, and system events using `ILogger`.
- **Input Validation**: Validates country codes (2-letter ISO) and duration (1-1440 minutes) using Regex and business rules.
- **SOLID Principles**: Services (`BlockingService`, `GeoLocationService`, `InMemoryRepository`) are loosely coupled and follow single responsibility.

## Challenges Overcome
1. **Rate Limit Error (HTTP 423)**:
   - Resolved by implementing caching in `GeoLocationService` to reduce calls to the ipgeolocation.io API.
   - Provided a fallback to ipwhois.io with updated configuration in `appsettings.json`.

2. **Incorrect `isBlocked` Response**:
   - Fixed the `GET /api/Ip/check-block` endpoint to return `IsBlocked` as a boolean instead of an object by extracting `Data` from `ApiResponse<bool>`.

3. **Missing Country Names**:
   - Replaced the faulty `GetCountryName` method (using a static IP) with an internal dictionary mapping country codes to names.

4. **Empty Blocked Countries List**:
   - Ensured `GET /api/Countries/blocked` returns correct data by validating `InMemoryRepository` logic and adding test data.

5. **Attempt Logging**:
   - Enabled logging of access attempts in `BlockedAttemptLog` for `GET /api/Ip/check-block` with details like `UserAgent`.

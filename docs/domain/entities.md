# Domain Entities

## User
- `Id` (Guid)
- `ClerkUserId` (string, unique index)
- `Email` (string, unique index)
- `Status` (UserStatus enum: stored as string)
- `Role` (UserRole enum: Customer | Admin, default Customer)
- `KycStatus` (KycStatus enum: Pending | InReview | Verified | Rejected)
- `StripeAccountId` (string?, added Phase 1)
- `EmailVerifiedAt` (DateTime?)
- `LastSignInAt` (DateTime?)
- `CreatedAt` / `UpdatedAt` / `ClosedAt`
- nav: `Profile` (UserProfile, one-to-one), `Identities` (ICollection<UserIdentity>)

## UserProfile
- `Id`, `UserId` (FK → User)
- `FirstName`, `LastName`, `DateOfBirth`, `NationalId`, `Country` (required)
- `AddressLine1/2`, `City`, `PostalCode`, `PhoneNumber` (optional)

## UserIdentity
- `Id`, `UserId` (FK → User)
- `Provider` (IdentityProvider: Email | Google | GitHub | Apple)
- `ProviderUserId` (string)

## Enums
- `KycStatus`: Pending → InReview → Verified | Rejected
- `UserStatus`: Active | (others TBD)
- `UserRole`: Customer | Admin
- `IdentityProvider`: Email | Google | GitHub | Apple

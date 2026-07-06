# Database Schema

Provider: PostgreSQL, snake_case naming, EF Core migrations.

## users
| column | type | notes |
|---|---|---|
| id | uuid | PK |
| clerk_user_id | varchar(255) | unique |
| email | varchar(255) | unique, check constraint (email regex) |
| role | varchar(30) | default 'Customer' |
| status | varchar(30) | |
| kyc_status | varchar(30) | Pending\|InReview\|Verified\|Rejected |
| stripe_account_id | varchar(255) | nullable, unique, added Phase 1 |
| email_verified_at | timestamptz | nullable |
| last_sign_in_at | timestamptz | nullable |
| created_at | timestamptz | |
| updated_at | timestamptz | |
| closed_at | timestamptz | nullable |

## user_profiles
| column | type | notes |
|---|---|---|
| id | uuid | PK |
| user_id | uuid | FK → users, cascade delete |
| first_name, last_name | varchar | required |
| date_of_birth | date | |
| national_id | varchar | |
| country | varchar | |
| address_line1/2, city, postal_code, phone_number | varchar | nullable |
| created_at / updated_at | timestamptz | |

## user_identities
| column | type | notes |
|---|---|---|
| id | uuid | PK |
| user_id | uuid | FK → users, cascade delete |
| provider | varchar | Email\|Google\|GitHub\|Apple |
| provider_user_id | varchar | |
| created_at / updated_at | timestamptz | |

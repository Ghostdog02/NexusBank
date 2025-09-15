# Nexus Bank 🏦

A modern, secure banking application built with Angular and Node.js, providing comprehensive financial services and account management capabilities.

## 🌟 Features

### 🔐 Authentication & Security
- Secure user registration and login
- JWT-based authentication
- Password encryption with bcrypt
- Session management and timeout


## 🚀 Tech Stack

### Frontend (Angular)
- **Framework**: Angular 17+
- **Styling**: Angular Material / Bootstrap
- **State Management**: NgRx / Angular Signals
- **HTTP Client**: Angular HttpClient
- **Forms**: Reactive Forms
- **Authentication**: JWT Guards

### Backend (Node.js)
- **Runtime**: Node.js 18+
- **Framework**: Express.js
- **Database**: MongoDB / PostgreSQL
- **Authentication**: JWT & Passport.js
- **Validation**: Joi / Express-validator
- **Security**: Helmet, CORS, Rate Limiting

## 📋 Prerequisites

Before running this application, make sure you have the following installed:

- **Node.js** (v18.0.0 or higher)
- **npm** (v8.0.0 or higher)
- **Angular CLI** (v17.0.0 or higher)
- **MongoDB** (v6.0 or higher) or **PostgreSQL** (v14.0 or higher)
- **Git**

```bash
# Check your versions
node --version
npm --version
ng version
```

## ⚡ Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/nexus-bank.git
cd nexus-bank
```

### 2. Backend Setup
```bash
# Navigate to backend directory
cd backend

# Install dependencies
npm install

# Create environment file
cp .env.example .env

# Update .env with your configurations
# DATABASE_URL, JWT_SECRET, etc.

# Start the development server
npm run dev
```

### 3. Frontend Setup
```bash
# Open new terminal and navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start the development server
ng serve
```

### 4. Access the Application
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:3000

## 🔧 Installation & Configuration

### Environment Variables

Create a `.env` file in the backend directory:

```env
# Server Configuration
PORT=3000
NODE_ENV=development

# Database
DATABASE_URL=mongodb://localhost:27017/nexusbank
# or for PostgreSQL: postgresql://username:password@localhost:5432/nexusbank

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key
JWT_EXPIRES_IN=7d

# Email Service (for notifications)
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USER=your-email@gmail.com
EMAIL_PASS=your-app-password

# External APIs
PLAID_CLIENT_ID=your-plaid-client-id
PLAID_SECRET=your-plaid-secret
STRIPE_SECRET_KEY=your-stripe-secret-key
```

### Database Setup

#### MongoDB
```bash
# Start MongoDB service
mongod

# Create database (optional - will be created automatically)
mongo nexusbank
```

### Frontend Testing
```bash
cd frontend

# Run unit tests
ng test

# Run e2e tests
ng e2e

# Generate coverage report
ng test --code-coverage
```

## 📦 Building for Production

### Backend
```bash
cd backend

# Build the application
npm run build

# Start production server
npm start
```

### Frontend
```bash
cd frontend

# Build for production
ng build --prod

# The build artifacts will be stored in the dist/ directory
```

## 🔒 Security Features

- **Data Encryption**: All sensitive data encrypted at rest and in transit
- **Input Validation**: Comprehensive input sanitization and validation
- **Rate Limiting**: API endpoint protection against abuse
- **CORS Protection**: Cross-origin request security
- **SQL Injection Prevention**: Parameterized queries and ORM protection
- **XSS Protection**: Content Security Policy implementation
- **Audit Logging**: Comprehensive transaction and access logging

### Code Style

- Follow Angular and Node.js best practices
- Use ESLint and Prettier for code formatting
- Write unit tests for new features
- Update documentation as needed

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.


---

**⭐ If you find this project useful, please give it a star on GitHub!**

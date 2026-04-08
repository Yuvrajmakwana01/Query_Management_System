# 📌 **Query Management System**

## 📖 **Overview**

The **Query Management System** is a full-stack web application designed to efficiently manage and track user queries within an organization.  
It provides a structured workflow for **Users, Employees, and Admins** to handle issues with proper tracking, status updates, and performance monitoring.

---

## 🚀 **Features**

### 👤 **User Management**
- **User Registration:** Register with company name, email, and password  
- **Secure Login:** Authentication-based access  
- **Role-Based Access Control:** Admin, Employee, User  

---

### 📝 **Query Management**
- **Create Queries with:**
  - Title  
  - Description  
  - Priority (**Low, Medium, High**)  

- **Track Query Status:**
  - Open  
  - In Progress  
  - Solved  

- **User Capabilities:**
  - Edit or delete queries (**only if unsolved**)  
  - Track submitted queries  

---

### 📊 **Dashboard (Admin & Employee)**
- **Query Statistics:**
  - Total queries (**All & Today**)  
  - Solved queries (**All & Today**)  
  - Pending queries (**All & Today**)  

- **Employee Performance Tracking:**
  - Number of queries resolved per employee  

---

## 👥 **Roles & Responsibilities**

### 🔑 **Admin**
- Manage users and queries  
- View complete dashboard and analytics  

---

### 👨‍💻 **Employee**
- View assigned/unsolved queries  
- Update query status (**Open → In Progress → Solved**)  
- Add comments/resolutions  
- Track personal performance  

---

### 🙋 **User**
- Submit queries  
- Edit/Delete queries (**if not solved**)  
- Track query status  

---

## 🛠️ **Tech Stack**

### 🔹 **Backend**
- ASP.NET Core MVC  
- REST APIs  
- JWT Authentication  

### 🔹 **Frontend**
- Kendo UI  
- HTML5, CSS3, JavaScript  

### 🔹 **Database**
- PostgreSQL  

### 🔹 **Tools & Technologies**
- Redis (**Caching**)  
- RabbitMQ (**Message Queue**)  
- ELK Stack (**Elasticsearch, Logstash, Kibana**)  

---

## 📁 **Project Structure**

```
QueryManagementSystem/
│
├── MVC/
│   ├── Controllers/
│   ├── Views/
│   ├── Models/
│
├── Repository/
│   ├── Interfaces/
│   ├── Implementations/
│
├── API/ (Optional if separated)
```

---

## ⚙️ **Setup Instructions**

### 1. Clone the repository
```
git clone https://github.com/Yuvrajmakwana01/Query_Management_System.git
cd Query_Management_System
```

---

### 2. Configure appsettings.json
```
{
  "ConnectionStrings": {
    "pgconn": "YOUR_POSTGRES_CONNECTION",
    "Redis": "YOUR_REDIS_CONNECTION"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY",
    "Issuer": "your-app",
    "Audience": "your-users"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

---

### 3. Run Required Services
- PostgreSQL  
- Redis  
- RabbitMQ  
- ELK Stack (Elasticsearch + Kibana)  

---

### 4. Run the Project
- Run MVC project  
- Run API (if separate)  

---

## 🔐 **Key Features**

- JWT-based authentication  
- Role-based authorization  
- Redis caching for performance optimization  
- RabbitMQ for asynchronous processing  
- ELK Stack for centralized logging and monitoring  
- Clean architecture (Controller → Service → Repository)  

---

## ⚡ **System Architecture**

- MVC interacts with backend APIs  
- Backend handles business logic and database operations  
- RabbitMQ processes background tasks  
- Redis improves performance through caching  
- ELK Stack handles logging and monitoring  

---

## 👨‍💻 **Author**

Yuvraj Makwana  

---

## ⭐ **Support**

If you like this project, give it a ⭐ on GitHub!
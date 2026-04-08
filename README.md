📌 Query Management System


📖 Overview

The Query Management System is a full-stack web application designed to efficiently manage and track user queries within an organization.
It provides a structured workflow for Users, Employees, and Admins to handle issues with proper tracking, status updates, and performance monitoring.


🚀 Features

👤 User Management :

User Registration: Register with company name, email, and password
Secure Login: Authentication-based access
Role-Based Access Control: Admin, Employee, User


📝 Query Management :

Create Queries with:
Title
Description
Priority (Low, Medium, High)

Track Query Status:
Open
In Progress
Solved

User Capabilities:
Edit or delete queries (only if unsolved)
Track submitted queries


📊 Dashboard (Admin & Employee) :

Query Statistics:
Total queries (All & Today)
Solved queries (All & Today)
Pending queries (All & Today)

Employee Performance Tracking:
Number of queries resolved per employee


👥 Roles & Responsibilities :

🔑 Admin :
Manage users and queries
View complete dashboard and analytics

👨‍💻 Employee :
View assigned/unsolved queries
Update query status (Open → In Progress → Solved)
Add comments/resolutions
Track personal performance

🙋 User :
Submit queries
Edit/Delete queries (if not solved)
Track query status


🛠️ Tech Stack :

🔹 Backend
ASP.NET Core MVC
REST APIs
JWT Authentication
🔹 Frontend
Kendo UI
HTML5, CSS3, JavaScript
🔹 Database
PostgreSQL
🔹 Tools & Technologies
Redis (Caching)
RabbitMQ (Message Queue)
ELK Stack (Elasticsearch, Logstash, Kibana)


🏗️ Project Structure :
MVC/
│
├── Controllers/
├── Views/
├── Models/
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/


🔐 Authentication & Security :

JWT-based authentication
Role-based authorization
Secure API communication


⚡ Key Highlights :

Scalable architecture using MVC pattern
Real-time query tracking and workflow
Performance optimization using Redis
Asynchronous communication using RabbitMQ
Logging and monitoring using ELK Stack


📌 Future Enhancements :

Email notifications for query updates
Real-time dashboard using WebSockets
File attachments in queries
Advanced analytics and reporting

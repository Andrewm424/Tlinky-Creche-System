🏫 Tlinky Crèche Management System

A community-focused full-stack project developed as part of Work Integrated Learning 3 (XADAD7112/w) at Rosebank College (IIE).
Designed to modernise the paper-based administration system at Tlinky Crèche, located in Ivory Park, Midrand.

🌟 Overview

The Tlinky Crèche Management System provides an integrated digital solution that simplifies daily operations for the crèche’s principal, teachers, and parents.

It consists of:

A Web Portal for the principal to manage children, staff, attendance, and payments.

A Mobile App for teachers and parents to capture attendance, share updates, and receive notifications.

🎯 Project Purpose

Tlinky Crèche previously relied on manual notebooks for attendance and fees, causing inefficiencies and miscommunication.
This system provides a secure, cloud-connected platform to automate records, improve accuracy, and strengthen parent–teacher communication.

🧩 System Components
Component	Description
Admin Web Portal	Built with ASP.NET Core MVC + Web API. Enables management of children, teachers, attendance, and fees.
Mobile App (Flutter)	Teachers record attendance and incidents; parents view child info, upload proof of payment, and receive notifications.
Database	PostgreSQL (Neon.tech) cloud database for reliability and scalability.
Storage	Cloudinary for image and document uploads.
Notifications	Firebase Cloud Messaging (FCM) for real-time alerts.
🛠️ Tech Stack

Backend: ASP.NET Core MVC + Web API

Frontend (Web): Bootstrap 5, Razor Views

Mobile: Flutter (Dart)

Database: PostgreSQL (Neon.tech)

Storage: Cloudinary

Notifications: Firebase Cloud Messaging

Version Control: GitHub + GitHub Actions (CI/CD)

🔐 Security & Compliance

POPIA-compliant data handling

Role-based authentication (Admin / Teacher / Parent)

HTTPS and token-based API security

Input validation and SQL injection protection


Web Portal	Mobile App

	
🗂️ Repository Structure

/Tlinky.AdminWeb/ → ASP.NET Core MVC portal
/Tlinky.MobileApp/ → Flutter teacher + parent app
/docs/ → UML, ERD, reports, mock-ups
/designs/ → Figma assets and screenshots
/.github/workflows/ → CI/CD build pipelines
README.md → Project documentation

🚀 Getting Started
1️⃣ Prerequisites

.NET 8 SDK

Flutter SDK

PostgreSQL Database (Neon.tech)

Cloudinary Account

2️⃣ Clone the Repository

git clone https://github.com/Andrewm424/Tlinky-Creche-System.git

cd Tlinky-Creche-System

3️⃣ Setup Backend

cd Tlinky.AdminWeb
dotnet restore
dotnet ef database update
dotnet run

4️⃣ Setup Mobile App

cd Tlinky.MobileApp
flutter pub get
flutter run

🧠 System Features
🖥️ Admin Web Portal

Manage children and staff records

Record attendance and fees

Upload photos via Cloudinary

Generate PDF / Excel reports

Post announcements to parents and teachers

📱 Mobile App

Teacher Side

Take daily attendance (online/offline)

Report incidents with photo evidence

Parent Side

View attendance history and child profile

Upload proof of payment

Receive fee reminders and updates

🧰 Tools and Services
Tool / Service	Purpose
Visual Studio 2022 / VS Code	IDE for web and API development
Postman	API testing
Neon.tech	Cloud PostgreSQL hosting
Render.com	Web API deployment
Cloudinary	Media storage for photos and proof of payments
GitHub Actions	Automated build and testing pipeline

🧾 Development Phases
Phase	Description	Status
Phase 1	Requirements Gathering	✅ Completed
Phase 2	Planning & Mock-ups	✅ Completed
Phase 3	System Design & Development	🔄 In Progress
Phase 4	Presentation & Demonstration	⏳ Pending
Phase 5	Final Documentation Submission	⏳ Pending
💬 Client Feedback

Mrs Dlamini Dzanibe (Principal of Tlinky Crèche) requested:

Colour-coded attendance view (Green = Present, Red = Absent)

Upload proof of payment function on mobile app

All feedback has been integrated into the Phase 3 development plan.

🧑‍💻 Developer

👤 Andrew Mukavela
Advanced Diploma in Application Development (IIE Rosebank College)
📍 Ivory Park, Midrand · South Africa



🏁 Acknowledgements

Special thanks to Mrs Dlamini Dzanibe, Principal of Tlinky Crèche, for her continued collaboration and feedback throughout this project.

📜 License

This project was developed for academic purposes under the
IIE Rosebank College – Work Integrated Learning 3 (XADAD7112/w) module.
© 2025 Andrew Mukavela · All Rights Reserved.

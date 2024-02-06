# Rentz

Rentz is a web application designed to facilitate the process of renting various types of living spaces, including apartments, villas, duplexes, and more. Built using ASP.NET Core and EF Code First, Rentz offers a user-friendly interface for both administrators and clients (who can also act as owners) to manage and rent properties seamlessly.

## Features

- **Property Management**: Admins can add, edit, and remove different types of living spaces available for rent, such as apartments, villas, and duplexes.
- **User Roles**: Rentz defines two primary user roles:
  - *Admin*: Administrators have access to the backend system for managing properties, users, and transactions.
  - *Client (Owner)*: Clients can list their properties for rent and manage their listings.
- **SignalR Integration**: Rentz utilizes SignalR for real-time chat functionality and notifications, enabling seamless communication between users and administrators.

## Technologies Used

- **ASP.NET Core**: A powerful and flexible framework for building web applications and APIs.
- **Entity Framework Core (EF)**: EF Code First approach is used for database management and migrations.
- **SignalR**: A library for adding real-time web functionality to applications, facilitating chat features and instant notifications.

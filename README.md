
# C# HTTP & HTTPS Proxy

This project implements a **high-performance HTTP and HTTPS proxy** using **C#**, designed for efficient request handling, logging, and authentication. It supports **URL logging**, **environment-variable-based authentication**, and seamless **Docker deployment**.

## 🚀 Key Features

- ✅ **Supports both HTTP & HTTPS traffic interception**
- ✅ **Logs all accessed URLs** to the terminal and a persistent log file
- ✅ **Enables authentication** via environment variables (`PROXY_USERNAME`, `PROXY_PASSWORD`)
- ✅ **Containerized deployment** using Docker for cross-platform compatibility
- ✅ **Optimized for performance and minimal resource utilization**

---

## 🎯 Use Cases

- **Security & Monitoring** → Track and log outgoing requests for auditing.
- **Traffic Filtering** → Restrict access to certain websites or services.
- **Performance Optimization** → Cache frequently accessed resources.
- **Development & Debugging** → Inspect HTTP/HTTPS requests for debugging applications.
- **Anonymization** → Route traffic through a controlled proxy for privacy.

---

## 🛠️ Installation & Usage Guide

### **1️⃣ Prerequisites**

Before running the proxy, ensure you have the following dependencies installed:

- **.NET SDK (9.0 or later)** → [Download .NET SDK](https://dotnet.microsoft.com/en-us/download)
- **Docker** (for containerized execution) → [Download Docker](https://www.docker.com/)

---

### **2️⃣ Running Locally (Without Docker)**

#### **🔹 Step 1: Clone the Repository**

```sh
git clone https://github.com/yourusername/csharp-proxy.git
cd csharp-proxy
```

#### **🔹 Step 2: Configure Environment Variables**

##### **Linux/macOS**

```sh
export PROXY_USERNAME=myuser
export PROXY_PASSWORD=mypass
```

##### **Windows (PowerShell)**

```powershell
$env:PROXY_USERNAME="myuser"
$env:PROXY_PASSWORD="mypass"
```

#### **🔹 Step 3: Restore Dependencies**

```sh
dotnet restore
```

#### **🔹 Step 4: Execute the Proxy**

```sh
dotnet run --project ProxyServer
```

#### **🔹 Step 5: Configure Your System to Use the Proxy**

Set your web browser, command-line tool, or application to route traffic through `http://localhost:8080`.

---

### **3️⃣ Running in a Docker Container**

#### **🔹 Step 1: Build the Docker Image**

```sh
docker build -t csharp-proxy .
```

#### **🔹 Step 2: Launch the Proxy in a Container**

```sh
docker run -d -p 8080:8080 \
  -e PROXY_USERNAME=myuser \
  -e PROXY_PASSWORD=mypass \
  --name proxy csharp-proxy
```

#### **🔹 Step 3: Validate Proxy Functionality**

Use `http://localhost:8080` as the proxy in your browser or command-line tool.

---

## 📂 Project Directory Structure

```
/csharp-proxy
│── ProxyServer/
│   ├── Program.cs          # Application entry point
│   ├── ProxyHandler.cs     # Handles HTTP and HTTPS requests
│   ├── Logger.cs           # Captures and records accessed URLs
│   ├── Authentication.cs   # Implements authentication mechanisms
│   ├── ProxyServer.csproj  # .NET project configuration
│── Dockerfile              # Docker build instructions
│── .gitignore              # Files and directories to be ignored by Git
│── README.md               # Project documentation
```

---

## 🔍 Logging & Monitoring

- **Live Terminal Logging** → Displays accessed URLs in real-time.
- **Persistent Log File (`proxy.log`)** → Stores all request URLs for auditing and analysis.

---

## 🎯 Troubleshooting Guide

| Issue                 | Potential Solution                                                                                 |
| --------------------- | -------------------------------------------------------------------------------------------------- |
| Proxy is unresponsive | Ensure your browser or application is correctly configured to use `http://localhost:8080`.       |
| Authentication fails  | Verify that the `PROXY_USERNAME` and `PROXY_PASSWORD` environment variables are correctly set. |
| Docker build errors   | Confirm that `.NET SDK 9` is installed inside the container environment.                         |

---

## 📜 Licensing Information

This project is released under the **MIT License**.

---

## ✨ Contribution Guidelines

We welcome contributions! Feel free to submit **bug reports, feature requests, or pull requests** on GitHub.

---

## 📞 Support & Contact

For inquiries or assistance, please open an issue on [GitHub Issues](https://github.com/yourusername/csharp-proxy/issues).

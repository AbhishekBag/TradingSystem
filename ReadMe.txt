# TradingSystem Project

## Overview
This project consists of two main components:

1. **TradingSystem**: A Service Fabric project that provides APIs for trading operations.
2. **TradingSystemSimulator_Console**: A console application created for simulation purposes, with various test cases present in `Program.cs`.

---

## 1. TradingSystem Service Fabric Project

### a. How to Instantiate Service Fabric Cluster

1. **Install Service Fabric SDK:**
   - Download and install the Service Fabric SDK from the [Microsoft Service Fabric website](https://learn.microsoft.com/en-us/azure/service-fabric/).

2. **Run Local Cluster:**
   - Set up a local cluster for a 1-node or 5-node configuration.

3. **Deploy the Application:**
   - Open the `TradingSystem` solution in Visual Studio (open in Admin mode).
   - Right-click on the `TradingSystem` project in the Solution Explorer and set it as the startup project.
   - Run the application.

### b. Using Swagger to Access APIs

1. **Run the Service Fabric Application:**
   - Ensure the Service Fabric cluster is running.
   - Deploy the `TradingSystem` application as described above.

2. **Access Swagger UI:**
   - Open a web browser and navigate to the Swagger UI endpoint. The URL is typically:
     ```
     https://localhost:8117/swagger/index.html
     ```
   - This will open the Swagger UI, where you can explore and test the different APIs exposed by the `TradingController`.

---

## 2. TradingSystemSimulator_Console

### a. Simulation Purpose
The `TradingSystemSimulator_Console` project is created for simulation purposes. It contains various test cases to simulate trading operations.

### b. Simulation Cases in `Program.cs`

1. **Open the Solution:**
   - Open the `TradingSystemSimulator_Console` solution in Visual Studio.

2. **Run the Console Application:**
   - Set `TradingSystemSimulator_Console` as the startup project.
   - Run the application by pressing `F5` or selecting `Debug > Start Debugging`.

3. **Simulation Cases:**
   - The simulation cases are present in the `Program.cs` file.
   - The console application will execute these cases and print the results to the console.

### Example Simulation Cases
- Adding dummy users.
- Placing buy and sell orders.
- Modifying and canceling orders.
- Querying order statuses.
- Printing all orders and active orders.

---

By following these steps, you can run the `TradingSystem` Service Fabric project and use the `TradingSystemSimulator_Console` for simulation purposes.


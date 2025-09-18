# **C\# Real-Time Order Book Viewer & Tick Simulator**

This project demonstrates a real-time, 50-level order book viewer with a separate tick simulator, built using C\# .NET 8 and WinForms. The architecture follows a client-server model where the OrderBookSimulator acts as a server, streaming updates to the OrderBookViewer client via a TCP connection.

## **Architecture**

* **OrderBookSimulator (Console Application):**  
  * Generates a live, 50-level bid/ask order book for a given symbol.  
  * Randomly applies small deltas to prices and quantities at a specified updates-per-second (bps).  
  * Sends **partial updates only** to the connected client, minimizing network traffic.  
  * Acts as a TCP server, listening for a single client connection.  
* **OrderBookViewer (WinForms Application):**  
  * A responsive WinForms UI that connects to the simulator.  
  * Displays the bids and asks in two DataGridView controls.  
  * Utilizes a background thread to ingest data from the TCP stream, preventing the UI from freezing.  
  * Batches updates to the UI thread to prevent flicker and maintain a smooth display.  
  * Highlights changed rows for a brief period to visualize real-time diffs.

## **How to Run**

### **Prerequisites**

You must have the .NET 8 SDK installed on your machine.

### **Steps**

1. **Open the Project:**  
   * Place both OrderBookSimulator.cs and OrderBookViewer.cs in the same directory.  
   * If using Visual Studio, create a new C\# Console App (.NET 8\) and a new C\# WinForms App (.NET 8\) in the same solution, then copy the code into the respective files.  
2. **Start the Simulator:**  
   * Open a command prompt or terminal.  
   * Navigate to the directory containing the OrderBookSimulator.cs file.  
   * Run the simulator with the desired symbol and updates-per-second (bps).  
   * **Example:** dotnet run \--project OrderBookSimulator \-- \--symbol="BTC/USD" \--bps=20  
   * The simulator will start and indicate that it is waiting for a client connection.  
3. **Start the Viewer:**  
   * Open another command prompt or terminal.  
   * Navigate to the directory containing the OrderBookViewer.cs file.  
   * Run the viewer application: dotnet run \--project OrderBookViewer  
   * A WinForms window will appear. The application will automatically attempt to connect to the simulator.  
4. **View the Order Book:**  
   * The viewer will display the live order book updates.  
   * Changes in price or quantity will be highlighted in green for a short period.  
   * The UI remains responsive, allowing you to resize the window or interact with other controls.

## **Metrics & Logging**

The simulator provides console output for its status. The viewer's UI includes a text box for logging and metrics to show real-time performance, such as dropped updates.
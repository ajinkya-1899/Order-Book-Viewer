# **C\# Real-Time Order Book Viewer & Tick Simulator**

This project demonstrates a real-time, 50-level order book viewer with a separate tick simulator, built using C\# .NET 9 and WinForms. The architecture follows a client-server model where the OrderBookSimulator acts as a server, streaming updates to the OrderBookViewer client via a TCP connection.

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


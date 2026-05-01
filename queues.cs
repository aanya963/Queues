// Basic Queue: Queue<T>
using System.Collections.Generic;
using System.Collections.Concurrent;

// 1. In-Memory Queues in C#
var queue = new Queue<string>();

queue.Enqueue("A");
queue.Enqueue("B");

var item = queue.Dequeue(); // "A"


// 2. Thread-Safe Queue: ConcurrentQueue<T>    
// Use this in multi-threaded scenarios:
var queue = new ConcurrentQueue<int>();

queue.Enqueue(1);
queue.TryDequeue(out int result);

// 3. Message Queues (Distributed Systems)
// These are used when:
//   You want decoupling
//   You need async processing
//   You handle high load or retries
// Popular Queue Systems in .NET ecosystem:
    // RabbitMQ
    // Apache Kafka
    // Azure Service Bus
    // Amazon SQS
// How Message Queues Work:
    // Producer sends message
    // Queue stores it
    // Consumer processes it
// [Producer] → [Queue] → [Consumer]

// 4. Queue Processing Patterns
// 🔹 1. Producer-Consumer Pattern
    // One part produces tasks
    // Another processes them
// 🔹 2. Work Queue (Load Distribution)
    // Multiple consumers process messages
// 🔹 3. Pub/Sub (Publish–Subscribe)
    // One message → multiple consumers
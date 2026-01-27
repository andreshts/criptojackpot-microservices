# 🎰 Diagrama de Procesos y Eventos WebSocket - CryptoJackpot

---

## 📋 Flujo Principal del Sistema

```mermaid
sequenceDiagram
    participant C as 🖥️ Cliente
    participant WS as 🔌 WebSocket Hub
    participant O as 📦 Order Service
    participant L as 🎟️ Lottery Service
    participant W as 💰 Wallet Service
    participant N as 🔔 Notification
    participant K as 📨 Kafka

    Note over C,K: 🛒 1. AGREGAR AL CARRITO Y RESERVAR NÚMEROS
    
    C->>WS: AddToCart (lotteryId, numbers[])
    WS->>O: CreateOrderCommand
    O->>K: OrderCreatedEvent
    K->>L: Consume OrderCreatedEvent
    L->>L: ReserveNumbers → RESERVED
    L->>K: NumbersReservedEvent
    K->>WS: Consume NumbersReservedEvent
    WS-->>C: ✅ OnNumbersReserved
    
    Note over L: ⏱️ Timer 10 min inicia
```

---

## ⏰ Flujo de Liberación por Timeout

```mermaid
sequenceDiagram
    participant L as 🎟️ Lottery Service
    participant K as 📨 Kafka
    participant O as 📦 Order Service
    participant WS as 🔌 WebSocket Hub
    participant C as 🖥️ Cliente
    participant N as 🔔 Notification

    Note over L,N: ⚠️ LIBERACIÓN AUTOMÁTICA (Si no se paga en 10 min)
    
    L->>L: CheckExpiredReservations
    L->>L: ReleaseExpiredNumbers
    L->>K: NumbersReleasedEvent
    K->>O: Consume NumbersReleasedEvent
    O->>O: CancelOrder → CANCELLED
    O->>K: OrderCancelledEvent
    K->>WS: Consume OrderCancelledEvent
    WS-->>C: ❌ OnOrderCancelled (timeout)
    K->>N: Consume OrderCancelledEvent
    N-->>C: 📱 "Reservación expirada"
```

---

## 💳 Flujo de Pago

```mermaid
sequenceDiagram
    participant C as 🖥️ Cliente
    participant WS as 🔌 WebSocket Hub
    participant W as 💰 Wallet Service
    participant K as 📨 Kafka
    participant O as 📦 Order Service
    participant L as 🎟️ Lottery Service
    participant N as 🔔 Notification

    Note over C,N: 💵 PROCESO DE PAGO

    C->>WS: InitiatePayment (orderId, wallet)
    WS->>W: ProcessPaymentCommand
    W->>W: ValidateTransaction (blockchain)
    
    alt ✅ Pago Confirmado
        W->>K: PaymentConfirmedEvent
        K->>O: Consume PaymentConfirmedEvent
        O->>O: UpdateOrderStatus → PAID
        O->>K: OrderPaidEvent
        K->>L: Consume OrderPaidEvent
        L->>L: ConfirmNumbers → SOLD
        L->>K: NumbersConfirmedEvent
        K->>WS: Consume NumbersConfirmedEvent
        WS-->>C: ✅ OnPaymentSuccess (txHash)
        WS-->>C: ✅ OnNumbersConfirmed
        K->>N: Consume OrderPaidEvent
        N-->>C: 📱 "¡Compra exitosa!"
    else ❌ Pago Fallido
        W->>K: PaymentFailedEvent
        K->>O: Consume PaymentFailedEvent
        O->>O: UpdateOrderStatus → PAYMENT_FAILED
        O->>K: OrderPaymentFailedEvent
        K->>WS: Consume OrderPaymentFailedEvent
        WS-->>C: ❌ OnPaymentFailed (error)
    end
```

---

## 🚫 Flujo de Cancelación Manual

```mermaid
sequenceDiagram
    participant C as 🖥️ Cliente
    participant WS as 🔌 WebSocket Hub
    participant O as 📦 Order Service
    participant K as 📨 Kafka
    participant L as 🎟️ Lottery Service

    Note over C,L: 🗑️ CANCELACIÓN MANUAL

    C->>WS: CancelOrder (orderId)
    WS->>O: CancelOrderCommand
    O->>K: OrderCancellationRequestedEvent
    K->>L: Consume OrderCancellationRequestedEvent
    L->>L: ReleaseNumbers → AVAILABLE
    L->>K: NumbersReleasedEvent
    K->>O: Consume NumbersReleasedEvent
    O->>O: UpdateOrderStatus → CANCELLED
    O->>K: OrderCancelledEvent
    K->>WS: Consume OrderCancelledEvent
    WS-->>C: ✅ OnOrderCancelled
```

---

## 🎫 Diagrama de Estados de los Números

```mermaid
stateDiagram-v2
    direction LR
    
    [*] --> Available
    
    Available --> Reserved : 🛒 AddToCart
    Reserved --> Available : ⏰ Timeout / 🚫 Cancel
    Reserved --> Sold : ✅ PaymentConfirmed
    Sold --> [*]
    
    state Available {
        [*] --> Libre
        Libre : 🟢 Disponible para reservar
    }
    
    state Reserved {
        [*] --> Bloqueado
        Bloqueado : 🟡 Timer 10 min activo
    }
    
    state Sold {
        [*] --> Vendido
        Vendido : 🔴 Estado final
    }
```

---

## 📦 Diagrama de Estados de la Orden

```mermaid
stateDiagram-v2
    direction LR
    
    [*] --> Pending : 📝 CreateOrder
    
    Pending --> Reserved : ✅ NumbersReserved
    Reserved --> AwaitingPayment : 💳 InitiatePayment
    AwaitingPayment --> Paid : ✅ PaymentConfirmed
    AwaitingPayment --> PaymentFailed : ❌ PaymentFailed
    Reserved --> Cancelled : ⏰ Timeout
    Reserved --> Cancelled : 🚫 UserCancel
    PaymentFailed --> Cancelled : 🔄 AutoCancel
    
    Paid --> [*]
    Cancelled --> [*]
    
    state Pending {
        [*] --> Creada
        Creada : 📋 Orden iniciada
    }
    
    state Reserved {
        [*] --> Reservada
        Reservada : 🎟️ Números bloqueados
    }
    
    state Paid {
        [*] --> Completada
        Completada : ✅ Compra exitosa
    }
    
    state Cancelled {
        [*] --> Cancelada
        Cancelada : ❌ Números liberados
    }
```

---

## 🔌 Eventos WebSocket (SignalR)

```mermaid
flowchart TB
    subgraph client["📤 Cliente → Servidor"]
        A1["🛒 AddToCart"]
        A2["🗑️ RemoveFromCart"]
        A3["💳 InitiatePayment"]
        A4["❌ CancelOrder"]
        A5["📊 GetLotteryStatus"]
        A6["🔔 SubscribeToLottery"]
    end
    
    subgraph server["📥 Servidor → Cliente"]
        B1["✅ OnNumbersReserved"]
        B2["🔓 OnNumbersReleased"]
        B3["🎟️ OnNumbersConfirmed"]
        B4["💰 OnPaymentSuccess"]
        B5["❌ OnPaymentFailed"]
        B6["🚫 OnOrderCancelled"]
        B7["📢 OnLotteryUpdated"]
        B8["⏰ OnReservationExpiring"]
        B9["🏆 OnWinnerAnnounced"]
    end
    
    subgraph broadcast["📡 Broadcast a Todos"]
        C1["📈 OnNumbersSoldUpdate"]
        C2["📊 OnLotteryProgress"]
        C3["🎬 OnDrawStarted"]
        C4["🎉 OnDrawCompleted"]
    end
```

---

## 🏗️ Arquitectura de Servicios

```mermaid
flowchart TB
    subgraph frontend["🖥️ Frontend"]
        UI["⚛️ React App"]
        WSC["🔌 WebSocket Client"]
    end
    
    subgraph gateway["🚪 Gateway"]
        NGINX["🌐 NGINX"]
        HUB["📡 SignalR Hub"]
    end
    
    subgraph services["⚙️ Microservicios"]
        ORDER["📦 Order"]
        LOTTERY["🎟️ Lottery"]
        WALLET["💰 Wallet"]
        WINNER["🏆 Winner"]
        NOTIF["🔔 Notification"]
        IDENTITY["👤 Identity"]
    end
    
    subgraph messaging["📨 Mensajería"]
        KAFKA["📬 Kafka"]
    end
    
    subgraph storage["💾 Almacenamiento"]
        PG[("🐘 PostgreSQL")]
        REDIS[("⚡ Redis")]
    end
    
    UI <-->|HTTP/WS| NGINX
    WSC <--> NGINX
    NGINX <--> HUB
    HUB <--> ORDER
    HUB <--> LOTTERY
    HUB <--> WALLET
    
    ORDER <--> KAFKA
    LOTTERY <--> KAFKA
    WALLET <--> KAFKA
    WINNER <--> KAFKA
    NOTIF <--> KAFKA
    
    ORDER --> PG
    LOTTERY --> PG
    WALLET --> PG
    WINNER --> PG
    
    ORDER --> REDIS
    LOTTERY --> REDIS
```

---

## 📬 Eventos de Kafka

| Evento | Productor | Consumidores | Descripción |
|:-------|:----------|:-------------|:------------|
| `OrderCreatedEvent` | 📦 Order | 🎟️ Lottery | Orden creada, reservar números |
| `NumbersReservedEvent` | 🎟️ Lottery | 📦 Order, 🔔 Notification | Números reservados exitosamente |
| `NumbersReleasedEvent` | 🎟️ Lottery | 📦 Order, 🔔 Notification | Números liberados (timeout/cancel) |
| `PaymentConfirmedEvent` | 💰 Wallet | 📦 Order, 🎟️ Lottery | Pago confirmado en blockchain |
| `PaymentFailedEvent` | 💰 Wallet | 📦 Order | Pago falló |
| `OrderPaidEvent` | 📦 Order | 🎟️ Lottery, 🔔 Notification, 🏆 Winner | Orden pagada completamente |
| `OrderCancelledEvent` | 📦 Order | 🔔 Notification | Orden cancelada |
| `LotteryCompletedEvent` | 🎟️ Lottery | 🏆 Winner | Lotería lista para sorteo |
| `WinnerSelectedEvent` | 🏆 Winner | 🔔 Notification, 💰 Wallet | Ganador seleccionado |
| `PrizeDistributedEvent` | 💰 Wallet | 🔔 Notification | Premio enviado al ganador |

---

## 📝 Resumen de Flujos

### 🛒 1. Agregar al Carrito
1. Usuario selecciona números
2. Se crea orden en estado `PENDING`
3. Lottery Service reserva los números (estado `RESERVED`)
4. Se inicia timer de 10 minutos
5. Usuario recibe confirmación via WebSocket

### 💳 2. Proceso de Pago
1. Usuario inicia pago con dirección de wallet
2. Wallet Service verifica transacción en blockchain
3. ✅ Si confirmado: Order → `PAID`, Numbers → `SOLD`
4. ❌ Si falla: Order → `PAYMENT_FAILED`, Numbers → `AVAILABLE`

### ⏰ 3. Liberación Automática
1. Background job verifica reservaciones cada minuto
2. Si expiró (10 min): Numbers → `AVAILABLE`, Order → `CANCELLED`
3. Usuario recibe notificación de expiración

### 🚫 4. Cancelación Manual
1. Usuario cancela orden
2. Numbers → `AVAILABLE`
3. Order → `CANCELLED`
4. Confirmación via WebSocket

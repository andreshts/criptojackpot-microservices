# Diagrama de Procesos y Eventos WebSocket - CryptoJackpot

## Flujo Principal del Sistema

```mermaid
sequenceDiagram
    participant C as Cliente (Frontend)
    participant WS as WebSocket Hub
    participant O as Order Service
    participant L as Lottery Service
    participant W as Wallet Service
    participant N as Notification Service
    participant K as Kafka Bus

    %% ========== FLUJO DE CARRITO ==========
    rect rgb(200, 230, 255)
        Note over C,K: 1. AGREGAR AL CARRITO Y RESERVAR NÚMEROS
        C->>WS: AddToCart (lotteryId, numbers[])
        WS->>O: CreateOrderCommand
        O->>K: OrderCreatedEvent
        K->>L: Consume OrderCreatedEvent
        L->>L: ReserveNumbers (marca números como RESERVED)
        L->>K: NumbersReservedEvent
        K->>WS: Consume NumbersReservedEvent
        WS-->>C: OnNumbersReserved (orderId, numbers[], expiresAt)
        
        Note over L: Timer de 10 minutos inicia
    end

    %% ========== FLUJO DE LIBERACIÓN POR TIMEOUT ==========
    rect rgb(255, 230, 200)
        Note over C,K: 2. LIBERACIÓN AUTOMÁTICA (Si no se paga)
        L->>L: CheckExpiredReservations (cada minuto)
        L->>L: ReleaseExpiredNumbers
        L->>K: NumbersReleasedEvent
        K->>O: Consume NumbersReleasedEvent
        O->>O: CancelOrder (status = CANCELLED)
        O->>K: OrderCancelledEvent
        K->>WS: Consume OrderCancelledEvent
        WS-->>C: OnOrderCancelled (orderId, reason: "timeout")
        K->>N: Consume OrderCancelledEvent
        N-->>C: Push Notification "Reservación expirada"
    end

    %% ========== FLUJO DE PAGO EXITOSO ==========
    rect rgb(200, 255, 200)
        Note over C,K: 3. PROCESO DE PAGO
        C->>WS: InitiatePayment (orderId, walletAddress)
        WS->>W: ProcessPaymentCommand
        W->>W: ValidateTransaction (blockchain)
        
        alt Pago Confirmado
            W->>K: PaymentConfirmedEvent
            K->>O: Consume PaymentConfirmedEvent
            O->>O: UpdateOrderStatus (PAID)
            O->>K: OrderPaidEvent
            K->>L: Consume OrderPaidEvent
            L->>L: ConfirmNumbers (status = SOLD)
            L->>K: NumbersConfirmedEvent
            K->>WS: Consume NumbersConfirmedEvent
            WS-->>C: OnPaymentSuccess (orderId, txHash)
            WS-->>C: OnNumbersConfirmed (numbers[])
            K->>N: Consume OrderPaidEvent
            N-->>C: Push Notification "Compra exitosa"
        else Pago Fallido
            W->>K: PaymentFailedEvent
            K->>O: Consume PaymentFailedEvent
            O->>O: UpdateOrderStatus (PAYMENT_FAILED)
            O->>K: OrderPaymentFailedEvent
            K->>WS: Consume OrderPaymentFailedEvent
            WS-->>C: OnPaymentFailed (orderId, error)
        end
    end

    %% ========== LIBERACIÓN MANUAL ==========
    rect rgb(255, 200, 200)
        Note over C,K: 4. CANCELACIÓN MANUAL
        C->>WS: CancelOrder (orderId)
        WS->>O: CancelOrderCommand
        O->>K: OrderCancellationRequestedEvent
        K->>L: Consume OrderCancellationRequestedEvent
        L->>L: ReleaseNumbers (status = AVAILABLE)
        L->>K: NumbersReleasedEvent
        K->>O: Consume NumbersReleasedEvent
        O->>O: UpdateOrderStatus (CANCELLED)
        O->>K: OrderCancelledEvent
        K->>WS: Consume OrderCancelledEvent
        WS-->>C: OnOrderCancelled (orderId)
    end
```

## Diagrama de Estados de los Números

```mermaid
stateDiagram-v2
    [*] --> AVAILABLE: Número creado
    
    AVAILABLE --> RESERVED: AddToCart / ReserveNumbers
    RESERVED --> AVAILABLE: Timeout (10 min) / CancelOrder
    RESERVED --> SOLD: PaymentConfirmed
    SOLD --> [*]: Número vendido permanentemente
    
    note right of RESERVED
        Timer de 10 minutos
        activo durante reserva
    end note
    
    note right of SOLD
        Estado final
        No puede cambiar
    end note
```

## Diagrama de Estados de la Orden

```mermaid
stateDiagram-v2
    [*] --> PENDING: CreateOrder
    
    PENDING --> RESERVED: NumbersReserved
    RESERVED --> AWAITING_PAYMENT: InitiatePayment
    AWAITING_PAYMENT --> PAID: PaymentConfirmed
    AWAITING_PAYMENT --> PAYMENT_FAILED: PaymentFailed
    RESERVED --> CANCELLED: Timeout / UserCancel
    PAYMENT_FAILED --> CANCELLED: AutoCancel
    
    PAID --> [*]: Orden completada
    CANCELLED --> [*]: Orden cancelada
```

## Eventos WebSocket (SignalR)

```mermaid
flowchart TB
    subgraph "Cliente → Servidor"
        A1[AddToCart]
        A2[RemoveFromCart]
        A3[InitiatePayment]
        A4[CancelOrder]
        A5[GetLotteryStatus]
        A6[SubscribeToLottery]
    end
    
    subgraph "Servidor → Cliente"
        B1[OnNumbersReserved]
        B2[OnNumbersReleased]
        B3[OnNumbersConfirmed]
        B4[OnPaymentSuccess]
        B5[OnPaymentFailed]
        B6[OnOrderCancelled]
        B7[OnLotteryUpdated]
        B8[OnReservationExpiring]
        B9[OnWinnerAnnounced]
    end
    
    subgraph "Broadcast a todos"
        C1[OnNumbersSoldUpdate]
        C2[OnLotteryProgress]
        C3[OnDrawStarted]
        C4[OnDrawCompleted]
    end
```

## Flujo de Integración de Servicios

```mermaid
flowchart LR
    subgraph Frontend
        UI[React/Vue App]
        WS[WebSocket Client]
    end
    
    subgraph Gateway
        NGINX[NGINX]
        HUB[SignalR Hub]
    end
    
    subgraph Microservices
        ORDER[Order Service]
        LOTTERY[Lottery Service]
        WALLET[Wallet Service]
        WINNER[Winner Service]
        NOTIF[Notification Service]
        IDENTITY[Identity Service]
    end
    
    subgraph MessageBus
        KAFKA[Kafka]
    end
    
    subgraph Storage
        PG[(PostgreSQL)]
        REDIS[(Redis Cache)]
    end
    
    UI <-->|HTTP/WS| NGINX
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

## Eventos de Kafka (Integration Events)

| Evento | Productor | Consumidores | Descripción |
|--------|-----------|--------------|-------------|
| `OrderCreatedEvent` | Order | Lottery | Orden creada, reservar números |
| `NumbersReservedEvent` | Lottery | Order, Notification | Números reservados exitosamente |
| `NumbersReleasedEvent` | Lottery | Order, Notification | Números liberados (timeout/cancel) |
| `PaymentConfirmedEvent` | Wallet | Order, Lottery | Pago confirmado en blockchain |
| `PaymentFailedEvent` | Wallet | Order | Pago falló |
| `OrderPaidEvent` | Order | Lottery, Notification, Winner | Orden pagada completamente |
| `OrderCancelledEvent` | Order | Notification | Orden cancelada |
| `LotteryCompletedEvent` | Lottery | Winner | Lotería lista para sorteo |
| `WinnerSelectedEvent` | Winner | Notification, Wallet | Ganador seleccionado |
| `PrizeDistributedEvent` | Wallet | Notification | Premio enviado al ganador |

## Resumen de Flujos

### 1. Agregar al Carrito
1. Usuario selecciona números
2. Se crea orden en estado PENDING
3. Lottery Service reserva los números (estado RESERVED)
4. Se inicia timer de 10 minutos
5. Usuario recibe confirmación via WebSocket

### 2. Proceso de Pago
1. Usuario inicia pago con dirección de wallet
2. Wallet Service verifica transacción en blockchain
3. Si confirmado: Order → PAID, Numbers → SOLD
4. Si falla: Order → PAYMENT_FAILED, Numbers → AVAILABLE

### 3. Liberación Automática
1. Background job verifica reservaciones cada minuto
2. Si expiró (10 min): Numbers → AVAILABLE, Order → CANCELLED
3. Usuario recibe notificación de expiración

### 4. Cancelación Manual
1. Usuario cancela orden
2. Numbers → AVAILABLE
3. Order → CANCELLED
4. Confirmación via WebSocket

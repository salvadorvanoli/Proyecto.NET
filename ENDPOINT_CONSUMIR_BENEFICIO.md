# Endpoint para Consumir Beneficios

## Descripción
Se ha implementado un nuevo endpoint que permite a los usuarios consumir beneficios. El endpoint crea registros de `Usage` y `Consumption` en la base de datos y actualiza las cuotas disponibles del beneficio.

## Endpoint

### POST `/api/Benefits/consume`

Consume un beneficio para el usuario autenticado.

#### Autenticación
Requiere token JWT válido. El endpoint extrae automáticamente el ID del usuario del token.

#### Request Body
```json
{
  "benefitId": 1,
  "quantity": 1
}
```

**Campos:**
- `benefitId` (int, requerido): ID del beneficio a consumir
- `quantity` (int, opcional): Cantidad a consumir (por defecto: 1, debe ser mayor a 0)

#### Responses

**200 OK - Éxito**
```json
{
  "usageId": 123,
  "consumptionId": 456,
  "benefitId": 1,
  "benefitTypeName": "Descuento en Cafetería",
  "quantityConsumed": 1,
  "remainingQuotas": 9,
  "consumptionDateTime": "2025-11-19T10:30:00Z",
  "message": "Beneficio 'Descuento en Cafetería' consumido exitosamente."
}
```

**400 Bad Request - Errores de validación o negocio**
```json
{
  "error": "El beneficio no está vigente."
}
```

Posibles mensajes de error:
- `"Usuario no encontrado."`
- `"Beneficio no encontrado."`
- `"El beneficio no está vigente."`
- `"El beneficio no tiene cuotas disponibles."`
- `"No hay suficientes cuotas disponibles. Cuotas disponibles: X"`
- `"La cantidad debe ser mayor a 0."`

**401 Unauthorized - Usuario no autenticado**
```json
{
  "error": "Usuario no autenticado."
}
```

**500 Internal Server Error**
```json
{
  "error": "Ocurrió un error al consumir el beneficio."
}
```

## Lógica de Negocio

El endpoint realiza las siguientes validaciones y operaciones:

1. **Validación de usuario**: Verifica que el usuario autenticado exista en el tenant
2. **Validación de beneficio**: Verifica que el beneficio exista en el tenant
3. **Validación de vigencia**: Verifica que el beneficio esté dentro de su período de validez
4. **Validación de cuotas**: Verifica que haya cuotas suficientes disponibles
5. **Creación de Usage**: Crea un registro de uso asociado al usuario y beneficio
6. **Creación de Consumption**: Crea un registro de consumo con fecha/hora
7. **Actualización de cuotas**: Decrementa las cuotas disponibles del beneficio

## Tablas Afectadas

- `Usages`: Se crea un nuevo registro
- `Consumptions`: Se crea un nuevo registro
- `Benefits`: Se actualizan las cuotas disponibles (campo `Quotas`)

## Ejemplo de Uso desde JavaScript/TypeScript

```javascript
const consumeBenefit = async (benefitId, quantity = 1) => {
  const response = await fetch('/api/Benefits/consume', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${yourJwtToken}`
    },
    body: JSON.stringify({
      benefitId: benefitId,
      quantity: quantity
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Error al consumir beneficio');
  }

  return await response.json();
};

// Uso
try {
  const result = await consumeBenefit(1, 1);
  console.log(`Beneficio consumido: ${result.message}`);
  console.log(`Cuotas restantes: ${result.remainingQuotas}`);
} catch (error) {
  console.error('Error:', error.message);
}
```

## Ejemplo de Uso desde C# (HttpClient)

```csharp
var request = new ConsumeBenefitRequest
{
    BenefitId = 1,
    Quantity = 1
};

var response = await httpClient.PostAsJsonAsync("/api/Benefits/consume", request);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadFromJsonAsync<ConsumeBenefitResponse>();
    Console.WriteLine($"Beneficio consumido: {result.Message}");
    Console.WriteLine($"Cuotas restantes: {result.RemainingQuotas}");
}
else
{
    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
    Console.WriteLine($"Error: {error.Error}");
}
```

## Archivos Modificados/Creados

### Nuevos archivos:
1. `src/Shared/DTOs/Benefits/ConsumeBenefitRequest.cs` - DTO para la solicitud
2. `src/Shared/DTOs/Benefits/ConsumeBenefitResponse.cs` - DTO para la respuesta
3. Este archivo de documentación

### Archivos modificados:
1. `src/Application/Benefits/IBenefitService.cs` - Agregado método `ConsumeBenefitAsync`
2. `src/Application/Benefits/BenefitService.cs` - Implementación del método `ConsumeBenefitAsync`
3. `src/Web.Api/Controllers/BenefitsController.cs` - Agregado endpoint POST `/api/Benefits/consume`

## Notas Importantes

- El endpoint NO modifica la estructura existente de beneficios
- Utiliza las entidades de dominio existentes: `Benefit`, `Usage`, `Consumption`
- Respeta las validaciones de dominio implementadas en `Benefit.ConsumeQuotas()`
- Es thread-safe gracias al uso de transacciones de Entity Framework
- El usuario se obtiene automáticamente del JWT token, no es necesario enviarlo en el body

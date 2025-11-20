# 🧪 Pruebas de Respuestas Visuales NFC

## 🎯 Objetivo de las Pruebas

Verificar que el usuario del teléfono con la credencial digital ve correctamente las respuestas visuales (✅ Permitido / ❌ Denegado) cuando pasa por un punto de control.

## 📋 Pre-requisitos

### Hardware
- ✅ 2 dispositivos Android con NFC
- ✅ NFC habilitado en ambos
- ✅ Android 4.4+ (API 19+)

### Software
- ✅ Backend corriendo (Web.Api en http://192.168.1.28:5000 o similar)
- ✅ Mobile.AccessPoint instalado en dispositivo 1
- ✅ Mobile.Credential instalado en dispositivo 2
- ✅ Usuarios creados en BD

### Base de Datos
```sql
-- Verificar que existen usuarios y puntos de control
SELECT * FROM Users;
SELECT * FROM ControlPoints;
SELECT * FROM AccessRules;
```

## 🧪 Casos de Prueba

### ✅ Prueba 1: Acceso Permitido - Usuario Autorizado

**Objetivo:** Verificar que se muestra ✅ ACCESO PERMITIDO

**Configuración:**
- Usuario: ID 2 (o cualquier usuario con permisos)
- Punto de Control: ID 1 (Entrada Principal)
- Horario: Dentro del permitido

**Pasos:**

1. **Dispositivo 1 (AccessPoint):**
   ```
   1. Abrir Mobile.AccessPoint
   2. Login con credenciales válidas
   3. Ir a página de NFC
   4. Configurar "Punto de Control ID" = 1
   5. Tocar "Iniciar Escucha NFC"
   6. Ver: "Escuchando..." (fondo verde claro)
   ```

2. **Dispositivo 2 (Credential):**
   ```
   1. Abrir Mobile.Credential
   2. Login con usuario ID 2
   3. Ver credencial digital
   4. Tocar "🚀 Activar Credencial"
   5. Ver: "Credencial activa - Acerca tu celular..."
   ```

3. **Contacto NFC:**
   ```
   1. Colocar los teléfonos back-to-back
   2. Mantener contacto 1-2 segundos
   3. Esperar respuesta
   ```

**Resultado Esperado en CREDENTIAL:**
```
┌─────────────────────────────┐
│          ✅                  │
│                             │
│   ACCESO PERMITIDO          │
│                             │
│   ✅ Acceso concedido       │
└─────────────────────────────┘
```
- ✅ Color de fondo: Verde (#00C853)
- ✅ Borde: Verde oscuro
- ✅ Aparece inmediatamente
- ✅ Desaparece después de 5 segundos

**Logs Esperados (Credential):**
```
🎯 NFC HCE: ACCESS RESPONSE RECEIVED
   Type: ✅ GRANTED
   Message: ✅ Acceso concedido
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Event fired to UI
📱 Access response received in service: GRANTED
🐛 Access response received in ViewModel: GRANTED - ✅ Acceso concedido
🟢 Showing ACCESS GRANTED UI
🔲 Access response hidden after 5 seconds
```

**Logs Esperados (AccessPoint):**
```
📤 Sending ACCESS GRANTED to credential device: ✅ Acceso concedido
✅ ACCESS GRANTED sent successfully
✅ Visual response sent to credential device successfully
```

---

### ❌ Prueba 2: Acceso Denegado - Sin Permisos

**Objetivo:** Verificar que se muestra ❌ ACCESO DENEGADO

**Configuración:**
- Usuario: ID 2
- Punto de Control: ID 5 (Área Restringida - sin permisos)
- Horario: Dentro del permitido

**Pasos:**

1. **Dispositivo 1 (AccessPoint):**
   ```
   1. Configurar "Punto de Control ID" = 5
   2. Tocar "Iniciar Escucha NFC"
   ```

2. **Dispositivo 2 (Credential):**
   ```
   1. Asegurar que credencial está activa
   ```

3. **Contacto NFC:**
   ```
   1. Acercar teléfonos back-to-back
   2. Mantener contacto 1-2 segundos
   ```

**Resultado Esperado en CREDENTIAL:**
```
┌─────────────────────────────┐
│          ❌                  │
│                             │
│   ACCESO DENEGADO           │
│                             │
│   ❌ Sin permiso para       │
│      este punto             │
└─────────────────────────────┘
```
- ✅ Color de fondo: Rojo (#D32F2F)
- ✅ Borde: Rojo oscuro
- ✅ Mensaje específico del motivo
- ✅ Desaparece después de 5 segundos

**Logs Esperados (Credential):**
```
🎯 NFC HCE: ACCESS RESPONSE RECEIVED
   Type: ❌ DENIED
   Message: ❌ Sin permiso para este punto
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📱 Access response received in service: DENIED
🔴 Showing ACCESS DENIED UI
```

---

### ❌ Prueba 3: Acceso Denegado - Horario No Permitido

**Objetivo:** Verificar denegación por horario

**Configuración:**
- Usuario: ID 2
- Punto de Control: ID 1
- Horario: Fuera del permitido (ej: 11:00 PM)

**Nota:** Para probar, temporalmente modificar regla de acceso:
```sql
UPDATE AccessRules 
SET StartTime = '08:00:00', EndTime = '17:00:00'
WHERE UserId = 2 AND ControlPointId = 1;
```
Luego probar fuera de este horario.

**Resultado Esperado:**
```
┌─────────────────────────────┐
│          ❌                  │
│   ACCESO DENEGADO           │
│   ❌ Horario no permitido   │
└─────────────────────────────┘
```

---

### 🔄 Prueba 4: Múltiples Pasadas Consecutivas

**Objetivo:** Verificar que funciona múltiples veces seguidas

**Pasos:**
1. Realizar Prueba 1 (acceso permitido)
2. Esperar 6 segundos (respuesta desaparece)
3. Repetir pasada inmediatamente
4. Verificar que respuesta aparece nuevamente

**Resultado Esperado:**
- ✅ Cada pasada muestra respuesta
- ✅ Respuestas no se solapan
- ✅ UI se limpia correctamente

---

### ⚠️ Prueba 5: Error de Conexión

**Objetivo:** Verificar comportamiento cuando backend no responde

**Configuración:**
1. Detener backend (Web.Api)
2. Mantener apps corriendo

**Pasos:**
1. Intentar pasar por punto de control
2. Esperar timeout

**Resultado Esperado en CREDENTIAL:**
```
┌─────────────────────────────┐
│          ❌                  │
│   ACCESO DENEGADO           │
│   ❌ Error de servidor      │
└─────────────────────────────┘
```

---

## 🐛 Troubleshooting

### Problema: No aparece respuesta visual

**Verificar:**

1. **NFC está activo:**
   ```
   - Ir a Configuración → Conexiones → NFC
   - Verificar que está ON
   ```

2. **HCE está configurado:**
   ```
   - Ir a Configuración → NFC → Pago sin contacto
   - Verificar que Mobile.Credential es la app por defecto
   ```

3. **Logs del HCE:**
   ```bash
   # Filtrar logs en Android Studio o via ADB
   adb logcat | grep "NFC HCE"
   
   # Buscar:
   # - "ACCESS RESPONSE RECEIVED"
   # - "Event fired to UI"
   ```

4. **Conexión ISO-DEP:**
   ```
   # En logs de AccessPoint buscar:
   adb logcat | grep "ISO-DEP"
   
   # Debe mostrar:
   # - "Connected to ISO-DEP tag"
   # - "ACCESS GRANTED sent successfully"
   ```

### Problema: Respuesta aparece pero no desaparece

**Verificar:**
- Timer de 5 segundos en `CredentialViewModel`
- Propiedad `ShowAccessResponse` se actualiza correctamente

**Fix:**
```csharp
// En OnAccessResponseReceived, verificar:
await Task.Delay(5000);
ShowAccessResponse = false;
```

### Problema: Mensaje incorrecto o vacío

**Verificar:**
- Encoding UTF-8 en ambos lados
- Longitud del mensaje < 200 caracteres
- No hay caracteres especiales problemáticos

**Logs:**
```bash
# En AccessPoint
adb logcat | grep "Sending ACCESS"

# En Credential
adb logcat | grep "Message:"
```

---

## 📊 Checklist de Pruebas

### Funcionalidad Básica
- [ ] Acceso permitido muestra ✅ verde
- [ ] Acceso denegado muestra ❌ rojo
- [ ] Mensaje correcto se muestra
- [ ] Respuesta desaparece después de 5 segundos
- [ ] UI vuelve a estado normal después

### Casos Edge
- [ ] Múltiples pasadas consecutivas funcionan
- [ ] Error de backend muestra mensaje de error
- [ ] Sin conexión NFC no crashea app
- [ ] Timeout de NFC se maneja correctamente

### UI/UX
- [ ] Colores son claramente distinguibles
- [ ] Texto es legible
- [ ] Tamaño de fuente apropiado
- [ ] Iconos se muestran correctamente (✅ ❌)
- [ ] Animación es fluida

### Performance
- [ ] Tiempo de respuesta < 1 segundo
- [ ] No hay lag en UI
- [ ] No hay memory leaks
- [ ] App no se calienta excesivamente

---

## 📈 Métricas de Éxito

| Métrica | Objetivo | Resultado |
|---------|----------|-----------|
| Tiempo de respuesta | < 1s | _________ |
| Tasa de éxito | > 95% | _________ |
| Claridad de mensaje | 100% legible | _________ |
| Satisfacción usuario | Alta | _________ |

---

## 🎬 Video de Prueba Sugerido

Grabar video mostrando:
1. Login en ambas apps
2. Activación de credencial
3. Inicio de escucha en AccessPoint
4. Contacto NFC
5. **Respuesta visual en pantalla de credencial** ⭐
6. Desaparición automática

---

## 📝 Reporte de Pruebas

### Prueba realizada por: _______________
### Fecha: _______________
### Dispositivos:
- AccessPoint: _______________
- Credential: _______________

### Resultados:

| Prueba | Resultado | Observaciones |
|--------|-----------|---------------|
| 1. Acceso Permitido | ☐ Pass ☐ Fail | |
| 2. Sin Permisos | ☐ Pass ☐ Fail | |
| 3. Horario Incorrecto | ☐ Pass ☐ Fail | |
| 4. Múltiples Pasadas | ☐ Pass ☐ Fail | |
| 5. Error Conexión | ☐ Pass ☐ Fail | |

### Notas adicionales:
_______________________________________________
_______________________________________________
_______________________________________________

---

## ✅ Firma de Aprobación

**Tester:** _______________  
**Fecha:** _______________  
**Estado:** ☐ Aprobado ☐ Requiere ajustes

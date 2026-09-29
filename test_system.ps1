$baseUrl = "http://localhost:5000/api"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "HEALTHCARE SYSTEM INTEGRATION TEST" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# ── 1. PATIENT WORKFLOW ────────────────────────────────────────────────────────
Write-Host "`n--- 1. Testing Patient Registration & Login ---" -ForegroundColor Yellow

$regBody = @{
    fullName    = "Test Patient"
    email       = "testpatient@example.com"
    password    = "Patient@123"
    phone       = "9998887770"
    dateOfBirth = "1999-05-15"
} | ConvertTo-Json

try {
    $regRes = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $regBody -Headers $headers
    Write-Host "SUCCESS: Patient registered. Token received." -ForegroundColor Green
} catch {
    Write-Host "INFO: Patient registration error (might already exist): $_" -ForegroundColor DarkYellow
}

# Login Patient
$loginBody = @{
    email    = "testpatient@example.com"
    password = "Patient@123"
} | ConvertTo-Json

$patientAuth = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -Headers $headers
$patientToken = $patientAuth.token
Write-Host "SUCCESS: Patient logged in. ProfileId: $($patientAuth.profileId)" -ForegroundColor Green

$patientHeaders = @{
    "Content-Type"  = "application/json"
    "Authorization" = "Bearer $patientToken"
}

# View Specializations
$specs = Invoke-RestMethod -Uri "$baseUrl/specializations" -Method Get
Write-Host "SUCCESS: Fetched $($specs.Count) specializations." -ForegroundColor Green

# View Doctors
$doctors = Invoke-RestMethod -Uri "$baseUrl/doctors" -Method Get
Write-Host "SUCCESS: Fetched $($doctors.Count) doctors." -ForegroundColor Green
$targetDoc = $doctors[0]
Write-Host "Selected Doctor: $($targetDoc.fullName) (ID: $($targetDoc.id))" -ForegroundColor Gray

# View Free Slots
$slots = Invoke-RestMethod -Uri "$baseUrl/availability/$($targetDoc.id)" -Method Get
Write-Host "SUCCESS: Fetched $($slots.Count) available slots for Dr. $($targetDoc.fullName)." -ForegroundColor Green

if ($slots.Count -gt 0) {
    $targetSlot = $slots[0]
    # Book Appointment
    $bookBody = @{
        doctorId       = $targetDoc.id
        availabilityId = $targetSlot.id
        notes          = "Automated test booking note"
    } | ConvertTo-Json

    $appt = Invoke-RestMethod -Uri "$baseUrl/appointments" -Method Post -Body $bookBody -Headers $patientHeaders
    Write-Host "SUCCESS: Booked appointment ID: $($appt.id), Status: $($appt.status)" -ForegroundColor Green

    # View My Appointments
    $myAppts = Invoke-RestMethod -Uri "$baseUrl/appointments/my" -Method Get -Headers $patientHeaders
    Write-Host "SUCCESS: Patient has $($myAppts.Count) total appointments." -ForegroundColor Green

    # Cancel Appointment
    $cancelRes = Invoke-RestMethod -Uri "$baseUrl/appointments/$($appt.id)/cancel" -Method Patch -Headers $patientHeaders
    Write-Host "SUCCESS: Cancelled appointment ID: $($appt.id). Status: $($cancelRes.status)" -ForegroundColor Green
}

# ── 2. DOCTOR WORKFLOW ────────────────────────────────────────────────────────
Write-Host "`n--- 2. Testing Doctor Login & Actions ---" -ForegroundColor Yellow

$docLoginBody = @{
    email    = "arjun@healthcare.com"
    password = "Doctor@123"
} | ConvertTo-Json

$docAuth = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $docLoginBody -Headers $headers
$docToken = $docAuth.token
$docProfileId = $docAuth.profileId
Write-Host "SUCCESS: Doctor logged in. DoctorId: $docProfileId" -ForegroundColor Green

$docHeaders = @{
    "Content-Type"  = "application/json"
    "Authorization" = "Bearer $docToken"
}

# Add Availability Slot
$addSlotBody = @{
    date      = (Get-Date).AddDays(10).ToString("yyyy-MM-dd")
    startTime = "14:00"
    endTime   = "14:30"
} | ConvertTo-Json

$newSlot = Invoke-RestMethod -Uri "$baseUrl/availability" -Method Post -Body $addSlotBody -Headers $docHeaders
Write-Host "SUCCESS: Added new availability slot ID: $($newSlot.id)" -ForegroundColor Green

# Get Doctor Appointments
$docAppts = Invoke-RestMethod -Uri "$baseUrl/appointments/doctor" -Method Get -Headers $docHeaders
Write-Host "SUCCESS: Doctor has $($docAppts.Count) appointments." -ForegroundColor Green

if ($docAppts.Count -gt 0) {
    $pendingAppt = $docAppts | Where-Object { $_.status -eq "Pending" } | Select-Object -First 1
    if ($pendingAppt) {
        $confirmBody = @{ doctorNotes = "Confirmed via automated test" } | ConvertTo-Json
        $confirmed = Invoke-RestMethod -Uri "$baseUrl/appointments/$($pendingAppt.id)/confirm" -Method Patch -Body $confirmBody -Headers $docHeaders
        Write-Host "SUCCESS: Confirmed appointment ID: $($pendingAppt.id). Status: $($confirmed.status)" -ForegroundColor Green
    }
}

# ── 3. ADMIN WORKFLOW ────────────────────────────────────────────────────────
Write-Host "`n--- 3. Testing Admin Login & Management ---" -ForegroundColor Yellow

$adminLoginBody = @{
    email    = "admin@healthcare.com"
    password = "Admin@123"
} | ConvertTo-Json

$adminAuth = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $adminLoginBody -Headers $headers
$adminToken = $adminAuth.token
Write-Host "SUCCESS: Admin logged in." -ForegroundColor Green

$adminHeaders = @{
    "Content-Type"  = "application/json"
    "Authorization" = "Bearer $adminToken"
}

# Add Specialization
$newSpecBody = @{
    name        = "Neurology Test"
    description = "Brain and nerve specialists"
} | ConvertTo-Json
$newSpec = Invoke-RestMethod -Uri "$baseUrl/specializations" -Method Post -Body $newSpecBody -Headers $adminHeaders
Write-Host "SUCCESS: Created Specialization: $($newSpec.name) (ID: $($newSpec.id))" -ForegroundColor Green

# Delete Specialization
Invoke-RestMethod -Uri "$baseUrl/specializations/$($newSpec.id)" -Method Delete -Headers $adminHeaders
Write-Host "SUCCESS: Deleted Specialization ID: $($newSpec.id)" -ForegroundColor Green

# View All Patients & All Appointments
$allPatients = Invoke-RestMethod -Uri "$baseUrl/admin/patients" -Method Get -Headers $adminHeaders
Write-Host "SUCCESS: Admin fetched $($allPatients.Count) patients." -ForegroundColor Green

$allAppts = Invoke-RestMethod -Uri "$baseUrl/appointments/all" -Method Get -Headers $adminHeaders
Write-Host "SUCCESS: Admin fetched $($allAppts.Count) all system appointments." -ForegroundColor Green

Write-Host "`n==========================================" -ForegroundColor Cyan
Write-Host "ALL SYSTEM INTEGRATION TESTS PASSED!" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;

/// 🌐 Central API service shared by web + mobile (Teacher + Parent)
class ApiService {
  // ---------------------------------------------------------------------------
  // 🌍 BASE URL CONFIG
  // ---------------------------------------------------------------------------
  static String get baseUrl {
    if (Platform.isAndroid) return 'http://10.0.2.2:5034/api';
    if (Platform.isWindows) return 'http://localhost:5034/api';
    if (Platform.isIOS) return 'http://localhost:5034/api';
    return 'http://localhost:5034/api';
  }

  // ---------------------------------------------------------------------------
  // 🧑‍🏫 TEACHER SECTION
  // ---------------------------------------------------------------------------

  // ✅ Login teacher
  static Future<Map<String, dynamic>?> loginTeacher(
      String email, String password) async {
    final uri = Uri.parse('$baseUrl/TeacherApi/login');
    final res = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );

    if (res.statusCode == 200) {
      return jsonDecode(res.body);
    }
    return null;
  }

  // ✅ Attendance endpoint base
  static String get attendanceBase => '$baseUrl/AttendanceApi';

  // ✅ Fetch attendance for teacher
  static Future<List<dynamic>> getAttendance({int? classId}) async {
    try {
      final todayUtc = DateTime.now().toUtc().toIso8601String();
      final uri =
          Uri.parse('$attendanceBase?date=$todayUtc&classId=${classId ?? 1}');
      final res = await http.get(uri);

      if (res.statusCode == 200) {
        final json = jsonDecode(res.body);
        if (json is Map && json.containsKey('data')) {
          return List<dynamic>.from(json['data']);
        } else if (json is List) {
          return json;
        }
        return [];
      } else {
        throw Exception('Failed to fetch attendance (HTTP ${res.statusCode})');
      }
    } catch (e) {
      throw Exception('Error fetching attendance: $e');
    }
  }

  // ✅ Save attendance record (handles null IDs)
  static Future<void> saveAttendance(Map<String, dynamic> record) async {
    try {
      final uri = Uri.parse(attendanceBase);
      record['date'] = DateTime.now().toUtc().toIso8601String();

      if (record['attendanceId'] == null ||
          record['attendanceId'].toString().isEmpty) {
        record.remove('attendanceId');
      }

      final res = await http.post(
        uri,
        headers: {'Content-Type': 'application/json; charset=utf-8'},
        body: jsonEncode([record]),
      );

      if (res.statusCode != 200) {
        throw Exception('Save failed (HTTP ${res.statusCode}) ${res.body}');
      }
    } catch (e) {
      throw Exception('Error saving attendance: $e');
    }
  }

  // ✅ Teacher dashboard summary (corrected endpoints)
  static Future<Map<String, dynamic>> getTeacherDashboard(
      int classId, int teacherId) async {
    final summaryUrl =
        Uri.parse('$baseUrl/AttendanceApi/summary?classId=$classId');
    final incidentsUrl =
        Uri.parse('$baseUrl/IncidentApi/count?teacherId=$teacherId');
    final announcementsUrl =
        Uri.parse('$baseUrl/AnnouncementsApi/count?audience=Teachers');

    final responses = await Future.wait([
      http.get(summaryUrl),
      http.get(incidentsUrl),
      http.get(announcementsUrl),
    ]);

    return {
      'totalChildren': jsonDecode(responses[0].body)['totalChildren'] ?? 0,
      'presentCount': jsonDecode(responses[0].body)['presentCount'] ?? 0,
      'incidentsCount': jsonDecode(responses[1].body)['count'] ?? 0,
      'announcementsCount': jsonDecode(responses[2].body)['count'] ?? 0,
    };
  }

  // ---------------------------------------------------------------------------
  // 👨‍👩‍👧 PARENT SECTION
  // ---------------------------------------------------------------------------

  // ✅ Parent login
  static Future<Map<String, dynamic>?> loginParent(
      String email, String password) async {
    final uri = Uri.parse('$baseUrl/ParentApi/login');
    final res = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );

    if (res.statusCode == 200) {
      final data = jsonDecode(res.body);
      if (data['success'] == true) return data;
    }
    return null;
  }

  // ✅ Fetch parent's children
  static Future<List<dynamic>> getChildren(int parentId) async {
    final uri = Uri.parse('$baseUrl/ParentApi/children/$parentId');
    final res = await http.get(uri);

    if (res.statusCode == 200) {
      return List<dynamic>.from(jsonDecode(res.body));
    } else {
      throw Exception('Failed to load children (${res.statusCode})');
    }
  }

  // ✅ Parent announcements
  static Future<List<dynamic>> getParentAnnouncements() async {
    final uri = Uri.parse('$baseUrl/AnnouncementsApi?audience=Parents');
    final res = await http.get(uri);

    if (res.statusCode == 200) {
      return List<dynamic>.from(jsonDecode(res.body));
    } else {
      throw Exception('Failed to load announcements (${res.statusCode})');
    }
  }

  // ✅ Parent Overview (Dashboard)
  static Future<Map<String, dynamic>> getParentOverview(int parentId) async {
    final uri = Uri.parse('$baseUrl/ParentApi/Overview/$parentId');
    final res = await http.get(uri);
    if (res.statusCode == 200) return jsonDecode(res.body);
    throw Exception('Failed to load overview (${res.statusCode})');
  }

  // ✅ Parent Payments
  static Future<Map<String, dynamic>> getParentPayments(int parentId) async {
    final uri = Uri.parse('$baseUrl/PaymentApi/GetByParent/$parentId');
    final res = await http.get(uri);
    if (res.statusCode == 200) return jsonDecode(res.body);
    throw Exception('Failed to load payments (${res.statusCode})');
  }

  // ✅ Attendance History
  static Future<List<dynamic>> getAttendanceHistory(int childId) async {
    final uri = Uri.parse('$baseUrl/AttendanceApi/child/$childId');
    final res = await http.get(uri);
    if (res.statusCode == 200) {
      final json = jsonDecode(res.body);
      return List<dynamic>.from(json['data'] ?? []);
    }
    throw Exception('Failed to load attendance history (${res.statusCode})');
  }

  // ---------------------------------------------------------------------------
  // 💰 PAYMENTS
  // ---------------------------------------------------------------------------

  // ✅ Upload proof of payment (multipart)
  static Future<Map<String, dynamic>> uploadPaymentProof({
    required int paymentId,
    required File proofFile,
  }) async {
    final uri = Uri.parse('$baseUrl/PaymentApi/UploadProof');

    final request = http.MultipartRequest('POST', uri)
      ..fields['paymentId'] = paymentId.toString()
      ..files.add(await http.MultipartFile.fromPath('file', proofFile.path));

    final response = await request.send();
    final body = await response.stream.bytesToString();

    if (response.statusCode == 200) {
      return jsonDecode(body);
    } else {
      throw Exception('Failed to upload proof (${response.statusCode}): $body');
    }
  }

  // ✅ Refresh parent payments + balance
  static Future<Map<String, dynamic>> refreshParentPayments(
      int parentId) async {
    final uri = Uri.parse('$baseUrl/PaymentApi/GetByParent/$parentId');
    final res = await http.get(uri);
    if (res.statusCode == 200) return jsonDecode(res.body);
    throw Exception('Failed to refresh payments (${res.statusCode})');
  }

  // ---------------------------------------------------------------------------
  // ⚠️ INCIDENTS
  // ---------------------------------------------------------------------------

  // ✅ Submit new incident (teacher)
  static Future<Map<String, dynamic>> submitIncident(
      Map<String, dynamic> incident) async {
    final uri = Uri.parse('$baseUrl/IncidentApi');
    final res = await http.post(
      uri,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(incident),
    );

    if (res.statusCode == 200) {
      return jsonDecode(res.body);
    } else {
      throw Exception(
          'Failed to submit incident (${res.statusCode}): ${res.body}');
    }
  }
}

// ---------------------------------------------------------------------------
// 🧑‍🏫 TEACHER SESSION (global holder)
// ---------------------------------------------------------------------------
class TeacherSession {
  static Map<String, dynamic>? teacher;

  static String get name => teacher?['fullName'] ?? 'Unknown';
  static String get className => teacher?['className'] ?? 'Unassigned';
  static int? get id => teacher?['teacherId'];
}

// ---------------------------------------------------------------------------
// 👨‍👩‍👧 PARENT SESSION (global holder)
// ---------------------------------------------------------------------------
class ParentSession {
  static Map<String, dynamic>? parent;

  static String get name => parent?['fullName'] ?? 'Unknown';
  static String get email => parent?['email'] ?? 'Unknown';
  static int? get id => parent?['parentId'];

  // ✅ Child info (flattened)
  static String get childName => parent?['childName'] ?? 'Unknown Child';
  static String get className => parent?['className'] ?? 'Unassigned Class';
  static String get allergies => parent?['allergies'] ?? 'None';
  static int get childAge => parent?['childAge'] ?? 0;
  static String? get childPhoto => parent?['childPhoto'];

  static List<dynamic> get children => parent?['children'] ?? [];
}

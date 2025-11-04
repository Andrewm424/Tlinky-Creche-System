import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_parent_drawer.dart';

class ParentMessagingScreen extends StatefulWidget {
  const ParentMessagingScreen({super.key});

  @override
  State<ParentMessagingScreen> createState() => _ParentMessagingScreenState();
}

class _ParentMessagingScreenState extends State<ParentMessagingScreen> {
  // Fixed teacher contact for this parent (no picker here)
  final String teacherName = 'Ms Ndlovu';
  final String teacherAvatar = 'assets/avatars/teacher.png';

  final List<Map<String, String>> messages = [
    {"sender": "Teacher", "content": "Hi! Reminder: art supplies tomorrow.", "time": "2025-09-12 08:30"},
    {"sender": "Parent", "content": "Thanks for the reminder!", "time": "2025-09-12 08:40"},
  ];

  final TextEditingController _controller = TextEditingController();

  void _send() {
    final text = _controller.text.trim();
    if (text.isEmpty) return;

    final now = DateFormat("yyyy-MM-dd HH:mm").format(DateTime.now());
    messages.add({"sender": "Parent", "content": text, "time": now});
    _controller.clear();
    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      drawer: const TlinkyParentDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        title: Row(
          children: [
            CircleAvatar(radius: 16, backgroundImage: AssetImage(teacherAvatar)),
            const SizedBox(width: 8),
            Text(teacherName, style: const TextStyle(fontWeight: FontWeight.w600)),
          ],
        ),
      ),
      body: Column(
        children: [
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: messages.length,
              itemBuilder: (context, i) {
                final msg = messages[i];
                final isParent = msg["sender"] == "Parent";
                return Align(
                  alignment: isParent ? Alignment.centerRight : Alignment.centerLeft,
                  child: Container(
                    margin: const EdgeInsets.symmetric(vertical: 6),
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: isParent ? AppTheme.primary.withOpacity(0.1) : Colors.grey.shade200,
                      borderRadius: BorderRadius.only(
                        topLeft: const Radius.circular(12),
                        topRight: const Radius.circular(12),
                        bottomLeft: isParent ? const Radius.circular(12) : Radius.zero,
                        bottomRight: isParent ? Radius.zero : const Radius.circular(12),
                      ),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(msg["content"]!, style: const TextStyle(fontSize: 14)),
                        const SizedBox(height: 4),
                        Text(msg["time"]!, style: const TextStyle(fontSize: 10, color: Colors.black54)),
                      ],
                    ),
                  ),
                );
              },
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
            decoration: const BoxDecoration(
              color: Colors.white,
              boxShadow: [BoxShadow(color: Colors.black12, blurRadius: 6, offset: Offset(0, -2))],
            ),
            child: SafeArea(
              top: false,
              child: Row(
                children: [
                  Expanded(
                    child: TextField(
                      controller: _controller,
                      decoration: InputDecoration(
                        hintText: 'Type a message…',
                        filled: true,
                        fillColor: Colors.grey.shade100,
                        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                        border: OutlineInputBorder(
                          borderRadius: BorderRadius.circular(20),
                          borderSide: BorderSide.none,
                        ),
                      ),
                      onSubmitted: (_) => _send(),
                    ),
                  ),
                  const SizedBox(width: 8),
                  IconButton(
                    icon: const Icon(Icons.send, color: AppTheme.primary),
                    onPressed: _send,
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

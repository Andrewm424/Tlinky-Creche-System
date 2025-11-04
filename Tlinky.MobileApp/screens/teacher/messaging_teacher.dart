import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../../theme/app_theme.dart';
import '../../widgets/tlinky_drawer.dart';

class ParentProfile {
  final String id;
  final String name;
  final String email;
  final String avatar;
  ParentProfile(this.id, this.name, this.email, this.avatar);
}

class TeacherMessagingScreen extends StatefulWidget {
  const TeacherMessagingScreen({super.key});

  @override
  State<TeacherMessagingScreen> createState() => _TeacherMessagingScreenState();
}

class _TeacherMessagingScreenState extends State<TeacherMessagingScreen> {
  final List<ParentProfile> parents = [
    ParentProfile('p1', 'Mr Chauke', 'chauke@tlinky.org', 'assets/avatars/parent1.png'),
    ParentProfile('p2', 'Mrs Mkhize', 'mkhize@tlinky.org', 'assets/avatars/parent2.png'),
    ParentProfile('p3', 'Mr Khumalo', 'khumalo@tlinky.org', 'assets/avatars/parent3.png'),
  ];

  ParentProfile? selectedParent;

  // message threads keyed by parent id
  final Map<String, List<Map<String, String>>> threads = {
    'p1': [
      {"sender": "Teacher", "content": "Good morning! Reminder: art supplies.", "time": "2025-09-12 08:30"},
      {"sender": "Parent", "content": "Noted, thank you!", "time": "2025-09-12 08:35"},
    ],
    'p2': [],
    'p3': [],
  };

  final TextEditingController _controller = TextEditingController();

  void _send() {
    if (selectedParent == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Select a parent first')),
      );
      return;
    }
    final text = _controller.text.trim();
    if (text.isEmpty) return;

    final now = DateFormat("yyyy-MM-dd HH:mm").format(DateTime.now());
    threads[selectedParent!.id] ??= [];
    threads[selectedParent!.id]!.add({
      "sender": "Teacher",
      "content": text,
      "time": now,
    });
    _controller.clear();
    setState(() {});
  }

  void _openParentPicker() {
    showModalBottomSheet(
      context: context,
      showDragHandle: true,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (_) => SafeArea(
        child: ListView.separated(
          padding: const EdgeInsets.all(16),
          itemCount: parents.length,
          separatorBuilder: (_, __) => const Divider(height: 1),
          itemBuilder: (_, i) {
            final p = parents[i];
            return ListTile(
              leading: CircleAvatar(backgroundImage: AssetImage(p.avatar)),
              title: Text(p.name, style: const TextStyle(fontWeight: FontWeight.w600)),
              subtitle: Text(p.email),
              onTap: () {
                Navigator.pop(context);
                setState(() => selectedParent = p);
              },
            );
          },
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final messages = selectedParent == null
        ? <Map<String, String>>[]
        : (threads[selectedParent!.id] ?? []);

    return Scaffold(
      drawer: const TlinkyDrawer(),
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        backgroundColor: AppTheme.primary,
        foregroundColor: Colors.white,
        elevation: 0,
        title: GestureDetector(
          onTap: _openParentPicker,
          child: Row(
            children: [
              CircleAvatar(
                radius: 16,
                backgroundImage: AssetImage(
                  selectedParent?.avatar ?? 'assets/avatars/parent1.png',
                ),
              ),
              const SizedBox(width: 8),
              Text(
                selectedParent?.name ?? 'Select Parent',
                style: const TextStyle(fontWeight: FontWeight.w600),
              ),
              const SizedBox(width: 4),
              const Icon(Icons.keyboard_arrow_down, size: 18),
            ],
          ),
        ),
      ),
      body: Column(
        children: [
          Expanded(
            child: selectedParent == null
                ? const Center(child: Text('Choose a parent to start chatting'))
                : ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: messages.length,
                    itemBuilder: (context, i) {
                      final msg = messages[i];
                      final isTeacher = msg["sender"] == "Teacher";
                      return Align(
                        alignment: isTeacher ? Alignment.centerRight : Alignment.centerLeft,
                        child: Container(
                          margin: const EdgeInsets.symmetric(vertical: 6),
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: isTeacher
                                ? AppTheme.primary.withOpacity(0.1)
                                : Colors.grey.shade200,
                            borderRadius: BorderRadius.only(
                              topLeft: const Radius.circular(12),
                              topRight: const Radius.circular(12),
                              bottomLeft: isTeacher ? const Radius.circular(12) : Radius.zero,
                              bottomRight: isTeacher ? Radius.zero : const Radius.circular(12),
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

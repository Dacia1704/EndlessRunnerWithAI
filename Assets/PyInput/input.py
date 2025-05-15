import cv2
import socket
import numpy as np
from cvzone.HandTrackingModule import HandDetector

# Khởi tạo camera và detector
cap = cv2.VideoCapture(0)
cap.set(3, 960) # Đặt kích thước của camera
cap.set(4, 960)
detector = HandDetector(maxHands=1)

# Socket kết nối đến Unity
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(('127.0.0.1', 5005))

# Thông số điều khiển
cooldown = 0
cooldown_threshold = 7
swipe_threshold = 60
track_length = 10
finger_tracks = []

# Vòng lặp chính
while True:
    success, img = cap.read()
    if not success:
        continue

    img = cv2.flip(img, 1)
    imgScaled = cv2.resize(img, (0, 0), None, 0.875, 0.875)

    if cooldown > 0:
        cooldown -= 1

    hands, img = detector.findHands(imgScaled)

    if hands and cooldown == 0:
        hand = hands[0]
        fingers = detector.fingersUp(hand)

        if sum(fingers) >= 1:  # Ít nhất một ngón mở (tránh khi nắm tay)
            middle_finger = hand["lmList"][8]
            finger_tracks.append(middle_finger)

            if len(finger_tracks) > track_length:
                finger_tracks.pop(0)

            start = finger_tracks[0]
            end = finger_tracks[-1]
            dx = end[0] - start[0]

            if abs(dx) > swipe_threshold:
                command = 1 if dx > 0 else 2
                print(f"Command: {command}")
                sock.sendall((f"{command}\n").encode('utf-8'))
                cooldown = cooldown_threshold
                finger_tracks = []
        else:
            finger_tracks = []


    # Hiển thị đường di chuyển (nếu có)
    if len(finger_tracks) > 1:
        for i in range(1, len(finger_tracks)):
            cv2.line(imgScaled,
                     (int(finger_tracks[i-1][0]), int(finger_tracks[i-1][1])),
                     (int(finger_tracks[i][0]), int(finger_tracks[i][1])),
                     (255, 0, 0), 2)

    cv2.putText(imgScaled, f"Cooldown: {cooldown}", (10, 30),
                cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0) if cooldown ==0 else (0,0,255)  , 2)

    cv2.imshow("Middle Finger Tracking", imgScaled)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
    if cv2.getWindowProperty("Middle Finger Tracking", cv2.WND_PROP_VISIBLE) < 1:
        break

cap.release()
cv2.destroyAllWindows()

# import mediapipe as mp
# print(mp.__path__)

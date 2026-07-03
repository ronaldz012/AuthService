import os
import smtplib
import ssl
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart


def load_env(path: str = ".env") -> None:
    with open(path) as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            key, _, val = line.partition("=")
            os.environ.setdefault(key.strip(), val.strip())


load_env()

HOST = os.getenv("SMTP_HOST", "smtp-relay.brevo.com")
PORT = int(os.getenv("SMTP_PORT", "587"))
USERNAME = os.environ["SMTP_USER"]
PASSWORD = os.environ["SMTP_PASS"]
SENDER = os.environ["SMTP_SENDER"]
RECIPIENT = os.environ["SMTP_RECIPIENT"]

msg = MIMEMultipart("alternative")
msg["Subject"] = "SMTP Test"
msg["From"] = SENDER
msg["To"] = RECIPIENT

text = MIMEText("This is a test email.", "plain")
html = MIMEText("<html><body><p>This is a test email.</p></body></html>", "html")
msg.attach(text)
msg.attach(html)

ctx = ssl.create_default_context()

with smtplib.SMTP(HOST, PORT, timeout=15) as server:
    server.ehlo()
    server.starttls(context=ctx)
    server.ehlo()
    server.login(USERNAME, PASSWORD)
    server.sendmail(SENDER, [RECIPIENT], msg.as_string())

print(f"✓ Email sent to {RECIPIENT}")

FROM python:3.13-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

ENV HOST=0.0.0.0
ENV FLASK_DEBUG=0

EXPOSE 5001

CMD ["python", "app.py"]

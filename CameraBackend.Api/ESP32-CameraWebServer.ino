#include "esp_camera.h"
#include "esp_http_server.h"
#include "esp_timer.h"
#include "WiFi.h"
#include "Arduino.h"

// WiFi credentials
const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

// Pin definitions for CAMERA_MODEL_AI_THINKER
#define PWDN_GPIO_NUM     32
#define RESET_GPIO_NUM    -1
#define XCLK_GPIO_NUM      0
#define SIOD_GPIO_NUM     26
#define SIOC_GPIO_NUM     27
#define Y9_GPIO_NUM       35
#define Y8_GPIO_NUM       34
#define Y7_GPIO_NUM       39
#define Y6_GPIO_NUM       36
#define Y5_GPIO_NUM       21
#define Y4_GPIO_NUM       19
#define Y3_GPIO_NUM       18
#define Y2_GPIO_NUM        5
#define VSYNC_GPIO_NUM    25
#define HREF_GPIO_NUM     23
#define PCLK_GPIO_NUM     22

// PIR Motion Sensor
#define PIR_PIN 13
#define LED_PIN 4

// HTTP Server and Stream handling
httpd_handle_t stream_httpd = NULL;
httpd_handle_t camera_httpd = NULL;

// Function declarations
void startCameraServer();
static esp_err_t stream_handler(httpd_req_t *req);
void setupCamera();
void setupWiFi();
void handleMotionDetection();

void setup() {
  Serial.begin(115200);
  
  // Initialize LED and PIR pins
  pinMode(LED_PIN, OUTPUT);
  pinMode(PIR_PIN, INPUT);
  
  setupCamera();
  setupWiFi();
  startCameraServer();
}

void loop() {
  handleMotionDetection();
  delay(100);
}

void setupCamera() {
  camera_config_t config;
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d0 = Y2_GPIO_NUM;
  config.pin_d1 = Y3_GPIO_NUM;
  config.pin_d2 = Y4_GPIO_NUM;
  config.pin_d3 = Y5_GPIO_NUM;
  config.pin_d4 = Y6_GPIO_NUM;
  config.pin_d5 = Y7_GPIO_NUM;
  config.pin_d6 = Y8_GPIO_NUM;
  config.pin_d7 = Y9_GPIO_NUM;
  config.pin_xclk = XCLK_GPIO_NUM;
  config.pin_pclk = PCLK_GPIO_NUM;
  config.pin_vsync = VSYNC_GPIO_NUM;
  config.pin_href = HREF_GPIO_NUM;
  config.pin_sscb_sda = SIOD_GPIO_NUM;
  config.pin_sscb_scl = SIOC_GPIO_NUM;
  config.pin_pwdn = PWDN_GPIO_NUM;
  config.pin_reset = RESET_GPIO_NUM;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG;

  // Initial settings
  config.frame_size = FRAMESIZE_VGA;
  config.jpeg_quality = 10;
  config.fb_count = 2;

  esp_err_t err = esp_camera_init(&config);
  if (err != ESP_OK) {
    Serial.printf("Camera init failed with error 0x%x", err);
    return;
  }
}

void setupWiFi() {
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.print("Camera Stream Ready! Go to: http://");
  Serial.println(WiFi.localIP());
}

static esp_err_t stream_handler(httpd_req_t *req) {
  camera_fb_t *fb = NULL;
  esp_err_t res = ESP_OK;
  char *part_buf[64];

  static int64_t last_frame = 0;
  if(!last_frame) {
    last_frame = esp_timer_get_time();
  }

  res = httpd_resp_set_type(req, _STREAM_CONTENT_TYPE);
  if(res != ESP_OK){
    return res;
  }

  httpd_resp_set_hdr(req, "Access-Control-Allow-Origin", "*");

  while(true) {
    fb = esp_camera_fb_get();
    if (!fb) {
      Serial.println("Camera capture failed");
      res = ESP_FAIL;
    } else {
      if(fb->format != PIXFORMAT_JPEG){
        bool jpeg_converted = frame2jpg(fb, 80, &_jpg_buf, &_jpg_buf_len);
        esp_camera_fb_return(fb);
        fb = NULL;
        if(!jpeg_converted){
          Serial.println("JPEG compression failed");
          res = ESP_FAIL;
        }
      } else {
        _jpg_buf_len = fb->len;
        _jpg_buf = fb->buf;
      }
    }

    if(res == ESP_OK) {
      res = httpd_resp_send_chunk(req, _STREAM_BOUNDARY, strlen(_STREAM_BOUNDARY));
    }
    if(res == ESP_OK) {
      size_t hlen = snprintf((char *)part_buf, 64, _STREAM_PART, _jpg_buf_len);
      res = httpd_resp_send_chunk(req, (const char *)part_buf, hlen);
    }
    if(res == ESP_OK) {
      res = httpd_resp_send_chunk(req, (const char *)_jpg_buf, _jpg_buf_len);
    }
    
    if(fb){
      esp_camera_fb_return(fb);
      fb = NULL;
      _jpg_buf = NULL;
    } else if(_jpg_buf){
      free(_jpg_buf);
      _jpg_buf = NULL;
    }
    if(res != ESP_OK){
      break;
    }
  }

  return res;
}

void startCameraServer() {
  httpd_config_t config = HTTPD_DEFAULT_CONFIG();
  config.server_port = 80;

  httpd_uri_t stream_uri = {
    .uri       = "/stream",
    .method    = HTTP_GET,
    .handler   = stream_handler,
    .user_ctx  = NULL
  };

  if (httpd_start(&stream_httpd, &config) == ESP_OK) {
    httpd_register_uri_handler(stream_httpd, &stream_uri);
  }
}

void handleMotionDetection() {
  if (di    if (!http.begin(client, "http://YOUR_BACKEND_IP:PORT/motion")) {
      Serial.println("Error: Failed to connect to backend server");
      digitalWrite(LED_PIN, LOW);
      return;
    }
    
    http.addHeader("Content-Type", "application/json");
    String payload = "{\"timestamp\":\"" + String(millis()) + "\"}";
    
    // Attempt to send POST request
    int httpResponseCode = http.POST(payload);
    
    if (httpResponseCode > 0) {
      String response = http.getString();
      Serial.printf("Motion detected! Server response code: %d\n", httpResponseCode);
      
      if (httpResponseCode == HTTP_CODE_OK) {
        Serial.println("Successfully notified backend");
      } else {
        Serial.printf("Warning: Unexpected response code: %d\n", httpResponseCode);
      }
    } else {
      Serial.printf("Error: HTTP POST failed, error: %s\n", http.errorToString(httpResponseCode).c_str());
yload = "{\"timestamp\":\"" + String(millis()) + "\"}";
    int httpResponseCode = http.POST(payload);
    
    if (httpResponseCode > 0) {
      Serial.println("Motion detected and notified!");
ed!");
tified!");
    }
    
    http.end();
    delay(1000); // Debounce
    digitalWrite(LED_PIN, LOW);
  }
}
#!/bin/bash

# Diagnostic Script for API 404 Errors
# Run this on your EC2 instance to diagnose why /api/auth/broker/login returns 404

echo "========================================="
echo "API 404 Error Diagnostic Script"
echo "========================================="
echo ""

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "1. Checking API Service Status..."
echo "-----------------------------------"
API_STATUS=$(sudo systemctl is-active insuranceloom-api.service 2>/dev/null)
if [ "$API_STATUS" = "active" ]; then
    echo -e "${GREEN}✓ API Service is RUNNING${NC}"
else
    echo -e "${RED}✗ API Service is NOT RUNNING (Status: $API_STATUS)${NC}"
    echo "   Attempting to start..."
    sudo systemctl start insuranceloom-api.service
    sleep 2
    API_STATUS=$(sudo systemctl is-active insuranceloom-api.service 2>/dev/null)
    if [ "$API_STATUS" = "active" ]; then
        echo -e "${GREEN}✓ API Service started successfully${NC}"
    else
        echo -e "${RED}✗ Failed to start API Service${NC}"
        echo "   Check logs: sudo journalctl -u insuranceloom-api.service -n 50 --no-pager"
    fi
fi
echo ""

echo "2. Checking if Port 5000 is Listening..."
echo "-----------------------------------"
PORT_CHECK=$(sudo ss -tulpn | grep :5000)
if [ -n "$PORT_CHECK" ]; then
    echo -e "${GREEN}✓ Port 5000 is listening${NC}"
    echo "   $PORT_CHECK"
else
    echo -e "${RED}✗ Port 5000 is NOT listening${NC}"
    echo "   API is not running or crashed"
fi
echo ""

echo "3. Testing Local API Endpoint..."
echo "-----------------------------------"
LOCAL_TEST=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/api/auth/broker/login -X POST -H "Content-Type: application/json" -d '{}' 2>/dev/null)
if [ "$LOCAL_TEST" = "400" ] || [ "$LOCAL_TEST" = "401" ] || [ "$LOCAL_TEST" = "200" ]; then
    echo -e "${GREEN}✓ Local API endpoint is reachable (HTTP $LOCAL_TEST)${NC}"
    echo "   (400/401/200 are expected - means endpoint exists)"
elif [ "$LOCAL_TEST" = "000" ] || [ "$LOCAL_TEST" = "" ]; then
    echo -e "${RED}✗ Cannot connect to local API (Connection refused)${NC}"
    echo "   API is not running on port 5000"
else
    echo -e "${YELLOW}⚠ Unexpected response: HTTP $LOCAL_TEST${NC}"
fi
echo ""

echo "4. Checking Nginx Status..."
echo "-----------------------------------"
NGINX_STATUS=$(sudo systemctl is-active nginx 2>/dev/null)
if [ "$NGINX_STATUS" = "active" ]; then
    echo -e "${GREEN}✓ Nginx is RUNNING${NC}"
else
    echo -e "${RED}✗ Nginx is NOT RUNNING${NC}"
    echo "   Starting Nginx..."
    sudo systemctl start nginx
    sleep 1
    NGINX_STATUS=$(sudo systemctl is-active nginx 2>/dev/null)
    if [ "$NGINX_STATUS" = "active" ]; then
        echo -e "${GREEN}✓ Nginx started successfully${NC}"
    fi
fi
echo ""

echo "5. Checking Nginx Configuration..."
echo "-----------------------------------"
if [ -f "/etc/nginx/sites-available/api.insuranceloom.com" ]; then
    echo -e "${GREEN}✓ Nginx config file exists${NC}"
    PROXY_PASS=$(grep -i "proxy_pass" /etc/nginx/sites-available/api.insuranceloom.com | grep -i "localhost:5000")
    if [ -n "$PROXY_PASS" ]; then
        echo -e "${GREEN}✓ proxy_pass to localhost:5000 found${NC}"
    else
        echo -e "${RED}✗ proxy_pass to localhost:5000 NOT found${NC}"
        echo "   Nginx may not be configured to proxy to API"
    fi
else
    echo -e "${YELLOW}⚠ Config file not found at /etc/nginx/sites-available/api.insuranceloom.com${NC}"
    echo "   Checking alternative locations..."
    find /etc/nginx -name "*api*" -o -name "*insuranceloom*" 2>/dev/null | head -5
fi
echo ""

echo "6. Testing Nginx Configuration..."
echo "-----------------------------------"
NGINX_TEST=$(sudo nginx -t 2>&1)
if echo "$NGINX_TEST" | grep -q "successful"; then
    echo -e "${GREEN}✓ Nginx configuration is valid${NC}"
else
    echo -e "${RED}✗ Nginx configuration has errors:${NC}"
    echo "$NGINX_TEST"
fi
echo ""

echo "7. Testing Public API Endpoint..."
echo "-----------------------------------"
PUBLIC_TEST=$(curl -s -o /dev/null -w "%{http_code}" https://api.insuranceloom.com/api/auth/broker/login -X POST -H "Content-Type: application/json" -d '{}' 2>/dev/null)
if [ "$PUBLIC_TEST" = "400" ] || [ "$PUBLIC_TEST" = "401" ] || [ "$PUBLIC_TEST" = "200" ]; then
    echo -e "${GREEN}✓ Public API endpoint is reachable (HTTP $PUBLIC_TEST)${NC}"
    echo "   (400/401/200 are expected - means endpoint exists)"
elif [ "$PUBLIC_TEST" = "404" ]; then
    echo -e "${RED}✗ Public API returns 404 (Not Found)${NC}"
    echo "   This is the error you're seeing!"
    echo "   Likely causes:"
    echo "   - Nginx not proxying correctly"
    echo "   - API not running"
    echo "   - Wrong Nginx configuration"
elif [ "$PUBLIC_TEST" = "502" ]; then
    echo -e "${RED}✗ Public API returns 502 (Bad Gateway)${NC}"
    echo "   Nginx can't reach the API backend"
    echo "   API is likely not running on port 5000"
else
    echo -e "${YELLOW}⚠ Unexpected response: HTTP $PUBLIC_TEST${NC}"
fi
echo ""

echo "8. Checking Recent API Logs..."
echo "-----------------------------------"
echo "Last 20 lines of API logs:"
sudo journalctl -u insuranceloom-api.service -n 20 --no-pager | tail -10
echo ""

echo "9. Summary and Recommendations"
echo "========================================="
if [ "$API_STATUS" != "active" ]; then
    echo -e "${RED}ACTION REQUIRED: Start the API service${NC}"
    echo "   sudo systemctl start insuranceloom-api.service"
    echo "   sudo systemctl enable insuranceloom-api.service"
    echo ""
fi

if [ -z "$PORT_CHECK" ]; then
    echo -e "${RED}ACTION REQUIRED: API is not listening on port 5000${NC}"
    echo "   Check API logs for errors:"
    echo "   sudo journalctl -u insuranceloom-api.service -n 50 --no-pager"
    echo ""
fi

if [ "$PUBLIC_TEST" = "404" ] && [ "$LOCAL_TEST" != "000" ]; then
    echo -e "${YELLOW}ISSUE: Local API works but public endpoint returns 404${NC}"
    echo "   This indicates an Nginx configuration problem"
    echo "   Check: sudo cat /etc/nginx/sites-available/api.insuranceloom.com"
    echo "   Restart: sudo systemctl restart nginx"
    echo ""
fi

if [ "$PUBLIC_TEST" = "502" ]; then
    echo -e "${RED}ISSUE: Bad Gateway - Nginx can't reach API${NC}"
    echo "   API is likely not running"
    echo "   Start API: sudo systemctl start insuranceloom-api.service"
    echo ""
fi

echo "========================================="
echo "Diagnostic complete!"
echo "========================================="

